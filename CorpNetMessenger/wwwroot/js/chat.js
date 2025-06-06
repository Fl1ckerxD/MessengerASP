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
hubConnection.on("Receive", function (message, attachments, userName, dateUtc) {
    // создаем элемент <b> для первой части сообщения
    const firstPartElem = document.createElement("b");
    firstPartElem.textContent = `${dateUtc} ${userName}: `;

    // создает элемент <p> для сообщения пользователя
    const elem = document.createElement("p");
    elem.appendChild(firstPartElem);
    elem.appendChild(document.createTextNode(message));
    if (attachments.length !== 0) {
        attachments.forEach(file => {
            const refFile = document.createElement("a");
            refFile.href = `/api/messages/download/${file.id}`;
            refFile.textContent = `${file.fileName}`;
            elem.appendChild(refFile);
        });
    }

    // добавляем новый элемент на страницу
    document.getElementById("chatroom").appendChild(elem);
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

    // редактирование сообщения
    const editBtns = document.querySelectorAll(".editBtn");
    editBtns.forEach(editBtn => {
        editBtn.addEventListener("click", () => {
            const messageElem = editBtn.closest("[data-message-id]");
            const messageId = messageElem.getAttribute("data-message-id");

            const messageText = messageElem.querySelector(".message-text").textContent.trim();
            const messageInput = document.getElementById("message");

            messageInput.value = messageText;

            messageInput.dataset.editingMessageId = messageId;
            messageInput.focus();
        });
    });

    // удаление сообщения
    const deleteBtns = document.querySelectorAll(".deleteBtn");
    deleteBtns.forEach(deleteBtn => {
        deleteBtn.addEventListener("click", async () => {
            const messageElem = deleteBtn.closest("[data-message-id]");
            const messageId = messageElem.getAttribute("data-message-id");
            const chatId = getChatIdFromUrl();

            // отправляем через SignalR
            await hubConnection.invoke("DeleteMessage", messageId, chatId);

            // удаляем сообщение из интерфейса
            messageElem.remove();
        });
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