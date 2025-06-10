// создание объекта для подключения
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// кнопка должна быть не активной пока не будет подключения к хабу
document.getElementById("sendBtn").disabled = true;

let skip = 5;
const take = 5;
let hasMoreMessages = true;

// получение нового сообщения
hubConnection.on("Receive", function (message) {
    const chatroom = document.getElementById("chatroom");

    const messageDiv = document.createElement("div");
    messageDiv.classList.add("message");
    messageDiv.setAttribute("data-message-id", message.id); // добавить ид сообщения
    messageDiv.innerHTML = `
        <div class="message-avatar">
                <i class="bi bi-person-fill"></i>
            </div>
            <div class="message-content">
                <div class="d-flex flex-column">
                    <div class="message-header">
                        <span class="message-author">${message.user.fullName}</span>
                        <span class="message-time">${message.sentAt}</span>
                    </div>
                    <p class="message-text">${message.text}</p>
                </div>
            </div>
    `;

    // добавляем вложения
    if (message.attachments.length > 0) {
        const attachmentsDiv = document.createElement("div");
        attachmentsDiv.classList.add("attachments");

        message.attachments.forEach(attachment => {
            if (attachment.isImage) {
                attachmentsDiv.innerHTML += `
                <div class="image-preview">
                        <img src="/api/messages/download/${attachment.id}"
                             alt="${attachment.fileName}"
                             class="message-image"
                             data-bs-toggle="modal"
                             data-bs-target="#imageModal"
                             data-src="/api/messages/download/${attachment.id}"
                             style="max-width: 300px; max-height: 300px;" />
                    </div>
                `;
            }
            else {
                attachmentsDiv.innerHTML += `
                    <div class="file-download">
                        <div class="file-icon">
                            <i class="bi bi-file-earmark"></i>
                        </div>
                        <div class="file-info">
                            <div class="file-name">${attachment.fileName}</div>
                            <div class="file-size">${attachment.fileSize}</div>
                        </div>
                        <a class="download-btn" href="/api/messages/download/${attachment.id}" download>
                            Скачать
                            <i class="bi bi-download"></i>
                        </a>
                    </div>
                    `;
            }
        });

        messageDiv.querySelector(".message-content .d-flex.flex-column").appendChild(attachmentsDiv);
    }

    // добавляем кнопки действий (если сообщение от текущего пользователя или админа)
    if (message.user.id === currentUserId || isAdminOrModerator()) {
        const actionsDiv = document.createElement("div");
        actionsDiv.classList.add("message-actions");

        if (message.user.id === currentUserId) {
            actionsDiv.innerHTML = `
                 <button class="editBtn" title="Редактировать">
                    <i class="bi bi-pencil-square"></i>
                </button>`;
        }

        actionsDiv.innerHTML += `
                 <button class="deleteBtn" title="Удалить">
                    <i class="bi bi-trash"></i>
                </button>`;

        messageDiv.querySelector(".message-content").appendChild(actionsDiv);
    }

    // добавляем сообщение в чат
    chatroom.appendChild(messageDiv);

    // прокручиваем вниз
    chatroom.scrollTop = chatroom.scrollHeight;
});

// получение отредактированного сообщения
hubConnection.on("UpdateMessage", function (messageId, newText) {
    const messageElem = document.querySelector(`[data-message-id="${messageId}"]`);

    if (messageElem) {
        // обновляем текст
        messageElem.querySelector(".message-text").textContent = newText
    }
});

// получение сигнала об удалении сообщения
hubConnection.on("RemoveMessage", (messageId) => {
    const messageElem = document.querySelector(`[data-message-id="${messageId}"]`);

    if (messageElem) {
        // удаляем элемент
        messageElem.remove();
    }
});

// получение ошибок
hubConnection.on("Error", displayError);

// получение истории сообщений
hubConnection.on("ReceiveHistory", (messages) => {
    const chatBox = document.getElementById("chatroom");

    // Сохраняем текущее состояние прокрутки
    const previousScrollHeight = chatBox.scrollHeight;
    const previousScrollTop = chatBox.scrollTop;

    if (messages.length === 0) {
        hasMoreMessages = false;
        console.log("Бошльше сообщений нет");
        return;
    }

    messages.forEach(message => {
        const messageEl = document.createElement("div");
        messageEl.setAttribute("data-message-id", message.id);
        let filesHtml = "";
        if (message.attachments && message.attachments.length > 0) {
            filesHtml = message.attachments.map(f =>
                `<a href="/api/messages/download/${f.id}">${f.name}</a>`
            ).join(", ");
        }

        messageEl.innerHTML = `
        <b>${new Date(message.sentAt).toLocaleString()}:</b> ${message.content}
        ${filesHtml ? `<div class="attachments">${filesHtml}</div>` : ""}
        `;

        chatBox.prepend(messageEl); // вставляем в начало - самые старые сверху
    });

    // Восстанавливаем позицию прокрутки
    const newScrollHeight = chatBox.scrollHeight;
    chatBox.scrollTop = previousScrollTop + (newScrollHeight - previousScrollHeight);
});

// подключение к хабу на сервере 
hubConnection.start()
    .then(function () {
        document.getElementById("sendBtn").disabled = false; // активируем кнопку отправления сообщения
        const chatId = getChatIdFromUrl(); // получаем id группы
        hubConnection.invoke("Enter", chatId); // подключаемся к группе
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

function displayError(error) {
    const errorElem = document.createElement("span");
    errorElem.textContent = error;
    errorElem.style.color = "red"; // Устанавливаем цвет текста

    document.getElementById("chatroom").appendChild(errorElem);

    // Удаляем через 5 секунд
    setTimeout(() => {
        chatroom.removeChild(errorElem);
    }, 5000);
};

function loadHistory() {
    if (!hasMoreMessages) return;

    const chatId = getChatIdFromUrl(); // получаем id группы

    hubConnection.invoke("LoadHistory", chatId, skip, take);
    skip += take;
};

function scrollToBottom() {
    const chatroom = document.getElementById("chatroom");
    if (chatroom) {
        chatroom.scrollTop = chatroom.scrollHeight;
    }
}

function isAdminOrModerator() {
    return currentUserRoles.includes("Admin") || currentUserRoles.includes("Mod");
}

let messageToDeleteId = null;

document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('fileInput');
    const addFileBtn = document.getElementById('addFileBtn');
    const fileList = document.getElementById('fileList');
    const imagePreviews = document.querySelectorAll('.message-image');
    let addedFiles = [];

    // передача изображения в модальное окно
    imagePreviews.forEach(img => {
        img.addEventListener('click', function () {
            const imageUrl = this.dataset.src;
            const fileName = imageUrl.split('/').pop(); // Получаем ID или имя файла из URL
            const downloadLink = document.getElementById('downloadLink');
            const modalImage = document.getElementById('modalImage');

            modalImage.src = imageUrl;

            // Установите ссылку и атрибут download
            downloadLink.href = imageUrl;
            downloadLink.setAttribute('download', fileName); // Браузер предложит сохранить как...
        });
    });

    // открыть диалог выбора файлов
    addFileBtn.addEventListener('click', () => {
        fileInput.click();
    });

    // обработка выбранных файлов
    fileInput.addEventListener('change', () => {
        const files = Array.from(fileInput.files)

        if (files.length > 3) {
            alert('Можно загрузить не более 3 файлов')
            return;
        }

        files.forEach(file => {
            addFileToList(file);
        })

        fileInput.value = ''; // Сброс выбора
    });

    // функция добавления файла в список
    function addFileToList(file) {
        if (fileList.childElementCount >= 3) {
            alert('Можно загрузить не более 3 файлов')
            return;
        }

        const fileDto = {
            id: "file-" + Date.now(),
            file: file,
        };
        addedFiles.push(fileDto);

        const li = document.createElement('li');
        li.innerHTML = `
        <span title="${file.name}">${truncateFileName(file.name)}</span>
        <button class="delete-btn" title="Удалить">&#10006;</button>
        `;

        // удаление файла
        li.querySelector('.delete-btn').addEventListener('click', () => {
            removeFile(fileDto.id);
            li.remove();
        });

        fileList.appendChild(li);
    }

    // удаление файла из списка
    function removeFile(fileId) {
        const index = addedFiles.findIndex(f => f.id === fileId);
        if (index !== -1) {
            addedFiles.splice(index, 1); // Удаляем 1 элемент начиная с index
        }
    }

    // ограничение длины имени файла
    function truncateFileName(name, maxLength = 20) {
        return name.length > maxLength ? name.substring(0, maxLength) + '...' : name;
    }

    // отправление сообщение на сервер по клику
    document.getElementById("sendBtn").addEventListener("click", sendMessage);

    // отправление сообщение на сервер по Enter
    document.getElementById("message").addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    async function sendMessage() {
        const textarea = document.getElementById("message");
        const messageText = textarea.value.trim();
        const chatId = getChatIdFromUrl();

        if (!messageText) return;

        const messageId = textarea.dataset.editingMessageId;

        if (messageId) {
            // режим редактирования
            await hubConnection.invoke("EditMessage", messageId, messageText, chatId);

            // очистка состояние редактирования
            delete textarea.dataset.editingMessageId;
            textarea.value = "";
            addedFiles = [];
            document.getElementById("fileList").innerHTML = "";
        }
        else {
            // режим нового сообщения
            const files = addedFiles.map(item => item.file)

            if (!messageText && addedFiles.length === 0) {
                alert("Введите сообщение или добавьте файл");
                return;
            }

            const formData = new FormData();
            formData.append("message", messageText);
            formData.append("chatId", chatId);

            // Добавляем все файлы в formData под ключом "files"
            for (let i = 0; i < files.length; i++) {
                formData.append("files", files[i]);
            }

            const response = await fetch("/api/messages/send", {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                // Очистка полей
                textarea.value = "";
                addedFiles = [];
                document.getElementById("fileList").innerHTML = "";
            }
            else {
                const errorMessage = await response.text();
                displayError(errorMessage);
            }
        }
    }

    document.getElementById("chatroom").addEventListener("click", (e) => {
        // редактирование сообщения
        const editBtn = e.target.closest(".editBtn");
        if (editBtn) {
            const messageElem = editBtn.closest("[data-message-id]");
            const messageId = messageElem.getAttribute("data-message-id");

            const messageText = messageElem.querySelector(".message-text").textContent.trim();
            const messageInput = document.getElementById("message");

            messageInput.value = messageText;

            messageInput.dataset.editingMessageId = messageId;
            messageInput.focus();
        }

        // открытие окна подтверждения при удалении сообщения
        const deleteBtn = e.target.closest(".deleteBtn");
        if (deleteBtn) {
            const messageElem = deleteBtn.closest("[data-message-id]");
            const messageId = messageElem.getAttribute("data-message-id");

            messageToDeleteId = messageId;
            const modal = new bootstrap.Modal(document.getElementById("confirmDeleteModal"));
            modal.show();
        }
    });

    // модальное окно подтверждения удаления сообщения
    document.getElementById("confirmDeleteBtn").addEventListener("click", async () => {
        if (!messageToDeleteId) return;

        const chatId = getChatIdFromUrl();

        // отправляем через SignalR
        await hubConnection.invoke("DeleteMessage", messageToDeleteId, chatId);

        // закрываем модальное окно
        bootstrap.Modal.getInstance(document.getElementById("confirmDeleteModal")).hide();

        // очищаем ID
        messageToDeleteId = null;
    });

    // добавление сообщений при прокрутки вверх
    document.getElementById("chatroom").addEventListener("scroll", function () {
        if (this.scrollTop === 0) {
            loadHistory();
        }
    });

    // после загрузки страницы ставим скролл на дефолтную позицию в самом низу
    window.addEventListener("load", scrollToBottom);
});

function getChatIdFromUrl() {
    const path = window.location.pathname;
    const parts = path.split('/');
    return parts[parts.length - 1];
}