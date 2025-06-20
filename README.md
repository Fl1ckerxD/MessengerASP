# 🚀 Переход с WPF на ASP.NET: Проект мессенджера

[![.NET](https://img.shields.io/badge/.NET_9.0-purple?logo=.net)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![EF Core](https://img.shields.io/badge/EF_Core-9.0-green)](https://learn.microsoft.com/ru-ru/ef/core/get-started/overview/install)
[![MS_SQL Server](https://img.shields.io/badge/MS_SQL_Server-2019+-orange)](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

> Этот репозиторий содержит работу по переходу десктоп-приложения (WPF) в веб-формат (ASP.NET). Цель — создание удобного корпоративного мессенджера для обмена сообщениями и файлами между сотрудниками организации.

## 📝 Описание проекта

**"CorpNet Messenger"** — это внутренний корпоративный мессенджер, который позволяет сотрудникам компании обмениваться текстовыми сообщениями и файлами в режиме реального времени. Первоначально разработан как WPF-приложение, теперь переписывается с использованием ASP.NET для перевода на веб-платформу.

### Основные функции:

- Обмен текстовыми сообщениями
- Отправка и получение файлов
- Авторизация и аутентификация пользователей
- Работа в группах/комнатах
- Уведомления о новых сообщениях
- История переписки

## 🔗 Исходный WPF-проект

Оригинальная реализация мессенджера на WPF доступна здесь:  
👉 [Ссылка на WPF-репозиторий](https://github.com/Fl1ckerxD/Messenger)

## 🛠️ Технологии

- **Frontend**: HTML, CSS, JavaScript, Razor Pages
- **Backend**: ASP.NET Core
- **База данных**: Entity Framework Core + MS SQL Server
- **Файловое хранилище**: локальное или облачное (в зависимости от реализации)
- **Дополнительно**: SignalR для чата в реальном времени

## 🗃️ Структура проекта

```
📦 CorpNetMessenger/
├── 📂 Domain/                  # Доменные модели и интерфейсы
│   ├── 📂 Entities/            # ORM-сущности
│   └── 📂 Interfaces/          # Сервисные интерфейсы
│
├── 📂 Infrastructure/          # Реализация репозиториев и сервисов
│   ├── 📂 Data/                # Контекст EF + миграции
│   ├── 📂 Repositories/        # Работа с БД
│   └── 📂 Services/            # Бизнес-логика
│
├── 📂 Web/                     # Веб-слои
│   ├── 📂 Controllers/         # MVC контроллеры
│   ├── 📂 ViewModels/          # Модели представления
│   └── 📂 Views/               # Razor-шаблоны
```

## 📸 Скриншоты (добавляются по мере реализации)

*(Сюда добавлю скриншоты интерфейса после реализации)*
### Авторизация
<img src="assets/screenshots/Login.png" width="500">

### Регистрация
<img src="assets/screenshots/Registration.png" width="500">

### Чат
<img src="assets/screenshots/Chat.png" width="500">

### Увеличенное изображение 
<img src="assets/screenshots/Enlarged_image.png" width="500">

### Редактирование профиля
<img src="assets/screenshots/Profile-editer.png" width="500">

## 📅 План доработки

- [x] Реализовать авторизацию через Identity  
- [x] Настроить SignalR для работы чата в реальном времени  
- [x] Реализовать загрузку и скачивание файлов  
- [ ] Добавить комнаты/чаты  
- [ ] Настроить фильтрацию и поиск сообщений  
- [ ] Создать админ-панель для управления пользователями  

## 📬 Связь

Если у вас есть вопросы или предложения, напишите мне:

- Email: mornival@outlook.com
- Telegram: @Fl1cker_0
