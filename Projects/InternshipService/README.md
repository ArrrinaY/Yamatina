# Internship and Job Service API

REST API сервис для управления стажировками и вакансиями с поддержкой ролевой авторизации, кэширования и полного CRUD функционала.

## Требования

- **.NET 8.0 SDK** или выше
- **Docker Desktop** (для Windows/Mac) или **Docker Engine** (для Linux)
- **Docker Compose** (обычно входит в Docker Desktop)

## Быстрый старт

### 1. Клонирование и подготовка

Убедитесь, что вы находитесь в директории проекта:
cd InternshipService

### 2. Запуск через Docker Compose

Запустите все сервисы (API, PostgreSQL, Redis, Liquibase):

docker-compose up --build

### 3. Проверка работы

### 1. Health Check

Проверьте здоровье приложения:

curl http://localhost:8080/health


Ожидаемый ответ: `Healthy` (статус 200)

### 2. Swagger UI

Откройте в браузере:
http://localhost:8080/swagger

Здесь вы можете:
- Просмотреть все доступные endpoints
- Протестировать API интерактивно
- Увидеть схемы данных и примеры запросов

## Аутентификация и авторизация

### Роли пользователей

1. **Admin** - полный доступ ко всем операциям (чтение, создание, обновление, удаление)
2. **Manager** - чтение, создание и обновление данных
3. **User** - чтение и создание данных

### Методы аутентификации

1. **JWT Bearer Token**
   - Получите токен через `/api/auth/login`:
    - Чтобы войти как админ:
        Введите в /api/auth/login 
        {
          "username": "admin",
          "password": "$2a$11$placeholder_hash_should_be_here"
        }
   - Используйте в заголовке: `Authorization: Bearer <token>`

2. **API Key**
   - Используйте в заголовке: `X-API-Key: <your-api-key>`
   - API Key настраивается в `appsettings.json`
   - По умолчанию: `your-api-key-here-change-in-production`

### Права доступа

| Право | Admin | Manager | User |
|-------|-------|---------|------|
| Read | ✅ | ✅ | ✅ |
| Create | ✅ | ✅ | ✅ |
| Update | ✅ | ✅ | ❌ |
| Delete | ✅ | ❌ | ❌ |

## Запуск тестов

### Unit-тесты репозиториев

cd InternshipService
dotnet test

Или для конкретного проекта тестов:
dotnet test InternshipService.Tests/InternshipService.Tests.csproj

## Дополнительная информация

### Структура проекта
InternshipService/
├── InternshipService/
│   ├── Controllers/        # API контроллеры
│   ├── Services/           # Бизнес-логика
│   ├── Repositories/       # Доступ к данным (EF Core + Dapper)
│   ├── Models/             # Entities и DTO
│   ├── Data/               # DbContext и миграции Liquibase
│   ├── Auth/               # JWT и API Key аутентификация
│   ├── Middleware/         # Обработка ошибок
│   ├── Mappings/           # AutoMapper профили
│   └── Validators/         # FluentValidation
├── InternshipService.Tests/  # Unit-тесты
├── docker-compose.yml      # Docker Compose конфигурация
└── Dockerfile              # Docker образ API

### Основные технологии

- **ASP.NET Core 8.0** - веб-фреймворк
- **PostgreSQL** - база данных
- **Entity Framework Core** - ORM
- **Dapper** - микро-ORM для производительности
- **Liquibase** - миграции БД
- **Redis** - кэширование
- **JWT Bearer** - аутентификация
- **API Key** - альтернативная аутентификация
- **Swagger/OpenAPI** - документация API
- **xUnit** - unit-тестирование
- **AutoMapper** - маппинг объектов
- **FluentValidation** - валидация
- **Serilog** - логирование

### Конфигурация

Основные настройки в `appsettings.json`:

- `Jwt` - настройки JWT токенов (Issuer, Audience, Key, ExpiryMinutes)
- `ApiKeySettings` - настройки API Key
- `ConnectionStrings:Postgres` - строка подключения к PostgreSQL
- `ConnectionStrings:Redis` - строка подключения к Redis
- `Serilog` - настройки логирования



