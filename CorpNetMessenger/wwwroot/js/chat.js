let hasMoreMessages = true;
let messageToDeleteId = null;
let addedFiles = [];
let skip = 20;
const take = 5;
const fileInput = document.getElementById('fileInput');
const addFileBtn = document.getElementById('addFileBtn');
const fileList = document.getElementById('fileList');

// создание объекта для подключения
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// кнопка должна быть не активной пока не будет подключения к хабу
document.getElementById("sendBtn").disabled = true;

// получение нового сообщения
hubConnection.on("Receive", function (message) {
    const chatroom = document.getElementById("chatroom");
    const messageDiv = createMessageDiv(message);

    // добавляем сообщение в чат
    chatroom.appendChild(messageDiv);

    // прокручиваем вниз
    chatroom.scrollTop = chatroom.scrollHeight;
    skip++;
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
        skip--;
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
        const messageDiv = createMessageDiv(message);
        chatBox.prepend(messageDiv); // вставляем в начало - самые старые сверху
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

// повторное подключение при обрыве
hubConnection.onclose(async () => {
    await new Promise(resolve => setTimeout(resolve, 5000));
    await hubConnection.start();
});

// отображение ошибок
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

// загрузка старых сообщений
function loadHistory() {
    if (!hasMoreMessages) return;

    const chatId = getChatIdFromUrl(); // получаем id группы

    hubConnection.invoke("LoadHistory", chatId, skip, take);
    skip += take;
};

// прокрутка чата к нижнему краю элемента
function scrollToBottom() {
    const chatroom = document.getElementById("chatroom");
    if (chatroom) {
        chatroom.scrollTop = chatroom.scrollHeight;
    }
}

// проверка прав
function isAdminOrModerator() {
    return currentUserRoles.includes("Admin") || currentUserRoles.includes("Mod");
}

// создание элемента сообщения
function createMessageDiv(message) {
    const messageDiv = document.createElement("div");
    messageDiv.classList.add("message");
    messageDiv.setAttribute("data-message-id", message.id); // добавить ид сообщения
    messageDiv.innerHTML = `
        <img class="message-avatar" src="/Avatar/${message.user.id}" alt="Аватар" />
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
    return messageDiv;
}

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

    const attachmentDiv = document.createElement('div');
    attachmentDiv.classList.add("attachment-container")
    attachmentDiv.innerHTML = `
            <div class="file-icon">
                <i class="bi bi-file-earmark"></i>
            </div>
            <div class="file-info">
                <div class="file-name">${truncateFileName(file.name)}</div>
                <div class="file-size">${formatBytes(file.size)}</div>
            </div>
            <button class="delete-attachment-btn" title="Удалить">&#10006;</button>
        `;

    // удаление файла
    attachmentDiv.querySelector('.delete-attachment-btn').addEventListener('click', () => {
        removeFile(fileDto.id);
        attachmentDiv.remove();
    });

    fileList.appendChild(attachmentDiv);
}

// удаление файла из списка
function removeFile(fileId) {
    const index = addedFiles.findIndex(f => f.id === fileId);
    if (index !== -1) {
        addedFiles.splice(index, 1); // Удаляем 1 элемент начиная с index
    }
}

// ограничение длины имени файла
function truncateFileName(name, maxLength = 13) {
    return name.length > maxLength ? name.substring(0, maxLength) + '...' : name;
}

// отправка сообщения
async function sendMessage() {
    const textarea = document.getElementById("message");
    const messageText = textarea.value.trim();
    const chatId = getChatIdFromUrl();

    const messageId = textarea.dataset.editingMessageId;

    if (messageId) {
        // режим редактирования
        if (!messageText) return;
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

// получение Id чата
function getChatIdFromUrl() {
    const path = window.location.pathname;
    const parts = path.split('/');
    return parts[parts.length - 1];
}

// конвертирование размера файла из байтов в килобайты, мегабайты, гигабайты и т.д.
function formatBytes(bytes) {
    if (bytes === 0) return "0 Byte";

    const suffixes = ["Byte", "KB", "MB", "GB", "TB", "PB", "EB"];
    const i = Math.floor(Math.log(Math.abs(bytes)) / Math.log(1024));

    let formatted = parseFloat((bytes / Math.pow(1024, i)).toFixed(1));

    return `${formatted} ${suffixes[i]}`;
}

document.addEventListener('DOMContentLoaded', () => {
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

    // отправление сообщение на сервер по клику
    document.getElementById("sendBtn").addEventListener("click", sendMessage);

    // отправление сообщение на сервер по Enter
    document.getElementById("message").addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

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

        // передача изображения в модальное окно
        if (e.target.classList.contains('message-image')) {
            const img = e.target.closest(".message-image");
            const imageUrl = img.dataset.src;
            const fileName = imageUrl.split('/').pop(); // Получаем ID или имя файла из URL
            const downloadLink = document.getElementById('downloadLink');
            const modalImage = document.getElementById('modalImage');

            modalImage.src = imageUrl;

            // Установите ссылку и атрибут download
            downloadLink.href = imageUrl;
            downloadLink.setAttribute('download', fileName); // Браузер предложит сохранить как...
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