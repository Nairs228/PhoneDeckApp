<div align="center">

# PhoneDeck Desktop

**Десктопное приложение для управления станциями сдачи телефонов**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com)
[![Avalonia](https://img.shields.io/badge/Avalonia_UI-11.x-8B5CF6?style=flat)](https://avaloniaui.net)
[![SQLite](https://img.shields.io/badge/SQLite-3-003B57?style=flat&logo=sqlite)](https://sqlite.org)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?style=flat&logo=windows)](https://microsoft.com/windows)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=flat&logo=csharp)](https://learn.microsoft.com/dotnet/csharp)

**Десктопная замена [веб-версии PhoneDeck](https://github.com/Nairs228/phonedeck)**

</div>

---

## Содержание

- [Архитектура](#архитектура)
- [Возможности](#возможности)
- [Установка и запуск](#установка-и-запуск)
- [Сборка установщика](#сборка-установщика)
- [Структура проекта](#структура-проекта)
- [Базы данных](#базы-данных)
- [Настройка окружения разработчика](#настройка-окружения-разработчика)
- [Устранение неполадок](#устранение-неполадок)
- [Известные ограничения](#известные-ограничения)

---

## Архитектура

### Схема системы

```
┌────────────────────────── СТАНЦИЯ ───────────────────────────┐
│                                                              │
│  [Телефон] → [Концевик] → [Arduino Uno] ──UART──► [ESP8266] │
│                                │                      │      │
│                         [Дисплей ММ:СС]          WiFi POST   │
└───────────────────────────────────────────────────┬──────────┘
                                                    ↓
                                       ┌─────────────────────┐
                                       │    Flask API-сервер  │
                                       │  http://109.73.206.169│
                                       │                      │
                                       │  POST /save          │
                                       │  GET  /get_data      │
                                       │  GET  /api/*         │
                                       │    ↓           ↓     │
                                       │  devices.db  users.db│
                                       └─────────────────────┘
                                                    ↑
                                       ┌─────────────────────┐
                                       │  PhoneDeck Desktop   │
                                       │                      │
                                       │  Avalonia UI (.NET 8)│
                                       │  SQLite (локально)   │
                                       │  MVVM Architecture   │
                                       └─────────────────────┘
```

### Стек технологий

| Слой | Технология | Роль |
|------|-----------|------|
| UI-фреймворк | Avalonia UI 11.x | Кроссплатформенный UI на AXAML |
| Язык | C# 12 / .NET 8 | Основной язык разработки |
| Архитектура | MVVM | CommunityToolkit.Mvvm (команды, биндинги) |
| БД | SQLite × 2 | `devices.db` — сессии, `users.db` — пользователи |
| Доступ к БД | Microsoft.Data.Sqlite | Работа с локальными базами данных |
| Хеширование | Werkzeug PBKDF2 | Совместимость с паролями от Flask-версии |

---

## Возможности

| Функция | Описание |
|---------|----------|
| 📊 **Дашборд** | Статистика сессий, пользователей, система бонусов |
| 📋 **История сессий** | Все подключения устройств с пагинацией |
| 👤 **Профиль** | Редактирование личных данных и смена пароля |
| 🎁 **Система бонусов** | 60 минут = 1 бонус, каталог наград, история трат |
| ⚙️ **Админ-панель** | Управление пользователями, состояние баз данных |
| 🔐 **Авторизация** | Вход, регистрация, функция «Запомнить меня» |
| 📡 **Мониторинг сети** | Уведомление при потере интернет-соединения |
| 🌙 **Тёмная тема** | Современный тёмный UI во всём приложении |
| 🖥️ **Кастомный titlebar** | Собственная строка заголовка в стиле приложения |

---

## Установка и запуск

### Вариант 1 — Готовый установщик (рекомендуется)

1. Перейди в раздел [Releases](../../releases)
2. Скачай `PhoneDeckSetup.exe`
3. Запусти и следуй инструкциям установщика
4. Ярлык появится на рабочем столе и в меню Пуск

**Системные требования:**
- Windows 10 / 11 (x64)
- 100 MB свободного места
- Интернет-соединение

---

### Вариант 2 — Из исходного кода

**Требования:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (с компонентом .NET desktop development)

```bash
# Клонировать репозиторий
git clone https://github.com/Nairs228/PhoneDeckApp.git
cd PhoneDeckApp

# Восстановить зависимости
dotnet restore

# Запустить приложение
dotnet run --project PhoneDeckApp
```

---

## Сборка установщика

### Шаг 1 — Собрать self-contained EXE

```bash
dotnet publish PhoneDeckApp -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o PhoneDeckApp/publish
```

После сборки скопировать базы данных в папку `publish/`:

```
PhoneDeckApp/publish/
├── PhoneDeckApp.exe   ← основной исполняемый файл
├── users.db           ← база пользователей
└── devices.db         ← база сессий устройств
```

### Шаг 2 — Собрать установщик через Inno Setup

1. Скачать и установить [Inno Setup](https://jrsoftware.org/isdl.php)
2. Открыть файл `installer.iss` в корне проекта
3. Нажать **Build → Compile** (или **F9**)
4. Готовый установщик появится в папке `installer_output/PhoneDeckSetup.exe`

---

## Структура проекта

```
PhoneDeckApp/
├── Models/
│   ├── User.cs                      # Модель пользователя
│   ├── Session.cs                   # Модель сессии устройства
│   └── BonusHistoryItem.cs          # Модель записи истории бонусов
│
├── Services/
│   ├── DatabaseService.cs           # Работа с SQLite (users.db + devices.db)
│   ├── SessionService.cs            # Текущий авторизованный пользователь
│   ├── PersistentSessionService.cs  # Сохранение сессии между запусками
│   └── NetworkService.cs            # Мониторинг интернет-соединения
│
├── ViewModels/
│   ├── ViewModelBase.cs             # Базовый класс ViewModel
│   ├── MainWindowViewModel.cs       # Навигация, логаут
│   ├── LoginViewModel.cs            # Экран входа
│   ├── RegisterViewModel.cs         # Экран регистрации
│   ├── DashboardViewModel.cs        # Дашборд + пагинация
│   ├── SessionsViewModel.cs         # История сессий + пагинация
│   ├── ProfileViewModel.cs          # Редактирование профиля
│   ├── BonusesViewModel.cs          # Бонусная система
│   └── AdminViewModel.cs            # Админ-панель
│
├── Views/
│   ├── MainWindow.axaml             # Главное окно с боковым меню и кастомным titlebar
│   ├── AuthWindow.axaml             # Окно авторизации/регистрации
│   ├── NoInternetWindow.axaml       # Окно при отсутствии интернета
│   ├── LoginView.axaml              # Форма входа
│   ├── RegisterView.axaml           # Форма регистрации
│   ├── DashboardView.axaml          # Дашборд
│   ├── SessionsView.axaml           # История сессий
│   ├── ProfileView.axaml            # Профиль пользователя
│   ├── BonusesView.axaml            # Система бонусов
│   └── AdminView.axaml              # Админ-панель
│
├── Assets/
│   └── icon.ico                     # Иконка приложения
│
├── App.axaml                        # Глобальные стили, ViewLocator
├── App.axaml.cs                     # Точка входа, навигация между окнами
├── ViewLocator.cs                   # Маппинг ViewModel → View
├── users.db                         # База данных пользователей
├── devices.db                       # База данных сессий устройств
├── installer.iss                    # Скрипт Inno Setup
└── PhoneDeckApp.csproj              # Файл проекта
```

---

## Базы данных

Приложение работает с теми же базами данных, что и веб-версия — файлы полностью совместимы.

### `users.db` — пользователи, таблица `users`

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | INTEGER PRIMARY KEY | Автоинкремент |
| `username` | TEXT UNIQUE | Логин |
| `password_hash` | TEXT | Хэш пароля (werkzeug PBKDF2) |
| `last_name` | TEXT | Фамилия |
| `first_name` | TEXT | Имя |
| `patronymic` | TEXT | Отчество |
| `phone_model` | TEXT | Модель телефона пользователя |
| `is_admin` | INTEGER | `0` = пользователь, `1` = администратор |

Дополнительные таблицы, создаваемые десктоп-версией:

| Таблица | Описание |
|---------|----------|
| `user_bonuses` | Баланс бонусов каждого пользователя |
| `bonus_history` | История трат бонусов |

### `devices.db` — сессии, таблица `devices`

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | INTEGER PRIMARY KEY | Автоинкремент |
| `name` | TEXT | ФИО |
| `model` | TEXT | Модель телефона |
| `charge` | TEXT | Заряд батареи |
| `connection_time` | TEXT | Время помещения в станцию (ЧЧ:ММ) |
| `disconnection_time` | TEXT | Время извлечения из станции (ЧЧ:ММ) |

> Таблицы `user_bonuses` и `bonus_history` создаются автоматически при первом запуске — вручную ничего создавать не нужно.

> ⚠️ Аккаунт администратора по умолчанию: `admin` / `adminkabphonedeck`

---

## Настройка окружения разработчика

### Необходимое ПО

| Программа | Где скачать |
|-----------|------------|
| .NET 8 SDK | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Visual Studio 2022 | [visualstudio.microsoft.com](https://visualstudio.microsoft.com) |
| Avalonia for VS 2022 | Extensions → Manage Extensions → поиск `Avalonia` |
| Inno Setup (для установщика) | [jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php) |

### Установка шаблонов Avalonia

```bash
dotnet new install Avalonia.Templates
```

### NuGet-пакеты проекта

| Пакет | Версия | Назначение |
|-------|--------|-----------|
| Avalonia | 11.x | UI-фреймворк |
| Avalonia.Desktop | 11.x | Поддержка десктопа |
| Avalonia.Themes.Fluent | 11.x | Тема Fluent |
| CommunityToolkit.Mvvm | 8.x | MVVM: команды, биндинги, ObservableProperty |
| Microsoft.Data.Sqlite | 8.x | Работа с SQLite |

### Запуск из Visual Studio

1. Открыть `PhoneDeckApp.sln`
2. Убедиться что `users.db` и `devices.db` лежат в папке проекта
3. В свойствах файлов БД: **Copy to Output Directory → Copy always**
4. Нажать **F5**

### Запуск из терминала

```bash
cd PhoneDeckApp
dotnet run
```

---

## Устранение неполадок

### Приложение не запускается — пустое окно

Убедиться что `users.db` и `devices.db` скопированы в папку:

```
bin\Debug\net8.0\
```

Или проверить свойство файла в Solution Explorer: **Copy to Output Directory → Copy always**.

### Ошибка `no such table: users`

SQLite-файл пустой или не тот. Проверить путь в Output окне VS (View → Output):

```
DB path: C:\...\bin\Debug\net8.0\users.db
```

Скопировать оригинальный файл по этому пути.

### Ошибка `Invalid salt version` при входе

Пароль в БД хешировался сторонней библиотекой (BCrypt), несовместимой с werkzeug. Приложение использует PBKDF2-совместимый алгоритм. Создать нового пользователя через кнопку **Зарегистрироваться**.

### Кириллица отображается знаками вопроса `????`

Файл `.axaml` сохранён не в UTF-8. Пересохранить:

**File → Save As → стрелка рядом с Save → Save with Encoding → Unicode (UTF-8 with signature) - Codepage 65001**

### Окно не появляется после нажатия «Выйти»

Скорее всего приложение закрылось вместе с главным окном. Убедиться что в `App.axaml.cs` при логауте новое окно показывается **до** закрытия старого:

```csharp
var oldWindow = desktop.MainWindow;
desktop.MainWindow = authWindow;
authWindow.Show();       // сначала показать новое
oldWindow?.Close();      // потом закрыть старое
```

### При отсутствии интернета приложение не запускается

Нормальное поведение — приложение требует интернет-соединение. Проверить подключение и перезапустить.

### Ошибка сборки `Unable to find type`

Не создан `.axaml.cs` code-behind файл для одного из View. Каждый `.axaml` должен иметь парный `.cs`:

```csharp
using Avalonia.Controls;

namespace PhoneDeckApp.Views;

public partial class МойView : UserControl
{
    public МойView() => InitializeComponent();
}
```

---

## Известные ограничения

1. **Только Windows.** Приложение собирается под `win-x64`. Для Linux/macOS нужно убрать флаг `-r win-x64` при публикации и проверить совместимость.

2. **Бонусы не синхронизируются с веб-версией.** В десктоп-приложении бонусы хранятся в локальных таблицах `user_bonuses` и `bonus_history`, которых нет в Flask API. При переносе БД на другой ПК бонусы сохраняются.

3. **Хеши паролей совместимы с Flask-версией** (PBKDF2 SHA-256), но зарегистрированные через десктоп пользователи не смогут войти через веб-версию без дополнительной настройки.

4. **Мониторинг интернета** проверяет доступность `8.8.8.8` (Google DNS) каждые 5 секунд через ICMP Ping. В корпоративных сетях с заблокированным ICMP это может давать ложные срабатывания.

5. **Рейтинг, статистика, станции, контакты** не реализованы (как и в веб-версии).

---

## Связанные проекты

- [PhoneDeck Web](https://github.com/Nairs228/phonedeck) — веб-версия на Flask + React + Arduino

---

<div align="center">

Сделано командой · ВУЗ · 2025–2026

</div>