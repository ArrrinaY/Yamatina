# Internship and Job Service API

REST API сервис для управления стажировками и вакансиями: компании публикуют вакансии, кандидаты подают заявки (applications), HR управляет статусами откликов.

## О проекте

**Предметная область:** платформа подбора стажировок и работы.

- Компании создают профили и публикуют вакансии
- Кандидаты подают заявки на вакансии
- Теги описывают навыки вакансий (many-to-many)
- Заявки проходят жизненный цикл: Applied → Reviewed → Accepted/Rejected

## Запуск

```bash
docker compose up --build
```

Сервисы:
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Health: http://localhost:8080/health
- PostgreSQL: localhost:54320

Перед первым запуском скопируйте `.env.example` в `.env`.

## Архитектура

```
Client (HTTP)
    ↓
Controllers (ASP.NET Core)
    ↓
Services (бизнес-логика)
    ↓
Repositories (EF Core + Dapper)
    ↓
PostgreSQL (+ Redis для кэша)
```

Миграции БД применяются автоматически через Liquibase при старте `docker compose up`.

## Схема БД

| Таблица | Описание | Связи |
|---------|----------|-------|
| `users` | Пользователи системы | — |
| `companies` | Компании-работодатели | 1→N `vacancies` |
| `vacancies` | Вакансии | N→1 `companies`, N↔M `tags` |
| `candidates` | Кандидаты | 1→N `applications` |
| `applications` | **Заявки на вакансии** | N→1 `candidates`, N→1 `vacancies` |
| `tags` | Теги навыков | N↔M `vacancies` через `vacancy_tags` |
| `vacancy_tags` | Связь вакансий и тегов | M↔M |
| `api_keys` | API-ключи | — |

## Основные endpoint'ы

| Метод | Endpoint | Описание |
|-------|----------|----------|
| GET | `/health` | Проверка API, PostgreSQL, Redis, партиций |
| GET/POST/PUT/DELETE | `/api/companies` | CRUD компаний |
| GET | `/api/companies/{id}/vacancies` | Вакансии компании |
| GET/POST/PUT/DELETE | `/api/vacancies` | CRUD вакансий (pagination + filters) |
| GET/POST/PUT/DELETE | `/api/candidates` | CRUD кандидатов |
| GET/POST/PUT/DELETE | `/api/applications` | CRUD заявок |
| GET | `/api/applications/vacancy/{id}` | Заявки по вакансии |
| GET | `/api/applications/candidate/{id}` | Заявки кандидата |
| GET | `/api/applications/candidate/{id}/filter` | Фильтрация заявок (status, from, to, page) |
| GET | `/api/applications/vacancy/{id}/statistics` | Агрегация заявок по статусам |
| GET/POST/PUT/DELETE | `/api/tags` | CRUD тегов |
| POST | `/api/auth/login` | JWT-аутентификация |

### Pagination и Filtering (vacancies)

```
GET /api/vacancies?page=1&pageSize=20&type=0&location=Moscow&isActive=true&companyId=1
```

### Filtering (applications)

```
GET /api/applications/candidate/1/filter?status=0&from=2026-01-01&to=2026-12-31&page=1&pageSize=20
```

## Основная сущность для масштабирования

**Таблица:** `applications`

**Почему она подходит:**
- Каждый кандидат может подать множество заявок
- Каждая вакансия получает поток откликов
- Объём растёт пропорционально числу кандидатов × вакансий
- Есть временное поле `applied_date` для партиционирования и аналитики
- Типичные запросы: фильтрация по кандидату/вакансии/дате/статусу

## Сложные SQL-запросы

### JOIN 1: заявки кандидата с компанией

```sql
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
INNER JOIN vacancies v ON a.vacancy_id = v.id
INNER JOIN companies co ON v.company_id = co.id
WHERE a.candidate_id = @CandidateId
ORDER BY a.applied_date DESC;
```

Используется в: `ApplicationRepository.GetByCandidateIdAsync`

### JOIN 2: заявки по вакансии с кандидатами

```sql
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
INNER JOIN candidates c ON a.candidate_id = c.id
INNER JOIN vacancies v ON a.vacancy_id = v.id
WHERE a.vacancy_id = @VacancyId
ORDER BY a.applied_date DESC;
```

Используется в: `ApplicationRepository.GetByVacancyIdAsync`

### Агрегирующий запрос: статистика заявок по статусам

```sql
SELECT a.status, COUNT(*) AS count
FROM applications a
INNER JOIN vacancies v ON a.vacancy_id = v.id
INNER JOIN companies c ON v.company_id = c.id
WHERE a.vacancy_id = @VacancyId
GROUP BY a.status
ORDER BY a.status;
```

Используется в: `ApplicationRepository.GetStatisticsByVacancyAsync`  
Endpoint: `GET /api/applications/vacancy/{id}/statistics`

## Генерация данных

Массовая генерация тестовых данных:

```bash
docker compose exec postgres psql -U postgres -d postgres -f /path/to/scripts/generate-test-data.sql
```

Или с хоста:

```bash
psql -h localhost -p 54320 -U postgres -d postgres -f scripts/generate-test-data.sql
```

Скрипт создаёт:
- 1 000 компаний
- 10 000 кандидатов
- 5 000 вакансий
- 100 000 заявок

## Лабораторные работы

Отчёты и SQL-скрипты находятся в папке `LAB/`:

- `LAB/lab-01-indexes/` — индексы и EXPLAIN ANALYZE
- `LAB/lab-02-scale/` — производительность при росте данных
- `LAB/lab-03-partitioning/` — партиционирование PostgreSQL

## Технологии

- ASP.NET Core 8.0, PostgreSQL 17, Redis, Liquibase
- EF Core + Dapper, JWT + API Key, Swagger, Serilog
- Партиционирование `applications` (RANGE по `applied_date`)
- Фоновые jobs: `CreatePartitionsJob`, `PartitionHealthMonitor`
