// создание объекта для подключения
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// кнопка должна быть не активной пока не будет подключения к хабу
document.getElementById("sendBtn").disabled = true;

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

// получение ошибок
hubConnection.on("Error", displayError);

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

document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('fileInput');
    const addFileBtn = document.getElementById('addFileBtn');
    const fileList = document.getElementById('fileList');
    let addedFiles = [];

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

    // отправление сообщение на сервер
    document.getElementById("sendBtn").addEventListener("click", async function () {
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
    });

    const editBtns = document.querySelectorAll(".editBtn");
    editBtns.forEach(editBtn => {
        // редактирование сообщения
        editBtn.addEventListener("click", () => {
            const messageElem = editBtn.closest("[data-message-id]");
            const messageId = messageElem.getAttribute("data-message-id");

            const messageText = messageElem.querySelector(".message-text").textContent.trim();
            const textarea = document.getElementById("message");

            textarea.value = messageText;

            textarea.dataset.editingMessageId = messageId;
        });
    });
});

function getChatIdFromUrl() {
    const path = window.location.pathname;
    const parts = path.split('/');
    return parts[parts.length - 1];
}