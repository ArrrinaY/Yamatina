-- ============================================================
-- Лабораторная №5. Шаг 2: создание таблицы сущности на шарде.
-- Выполняется ОДИНАКОВО на каждом шарде:
--   docker exec -i lab5-sharding-shard0-1 psql -U postgres -d postgres -v shard=0 < 01_setup_shards.sql
--   docker exec -i lab5-sharding-shard1-1 psql -U postgres -d postgres -v shard=1 < 01_setup_shards.sql
--   docker exec -i lab5-sharding-shard2-1 psql -U postgres -d postgres -v shard=2 < 01_setup_shards.sql
-- ============================================================

-- Реальная сущность сервиса: applications.
-- Shard key: candidate_id — по нему сервис чаще всего фильтрует заявки.
CREATE TABLE applications (
    id           integer PRIMARY KEY,
    candidate_id integer  NOT NULL,
    vacancy_id   integer  NOT NULL,
    status       smallint NOT NULL DEFAULT 0,
    applied_date timestamptz NOT NULL DEFAULT now(),
    cover_letter text
);

CREATE INDEX idx_applications_candidate_id ON applications (candidate_id);
CREATE INDEX idx_applications_vacancy_id   ON applications (vacancy_id);

SELECT current_database() AS database,
       inet_server_port() AS port,
       :shard AS shard_index,
       to_regclass('public.applications') AS applications_table;
