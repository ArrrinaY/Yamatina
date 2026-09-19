-- Лабораторная 2: таблица events для экспериментов с ростом данных
CREATE SCHEMA IF NOT EXISTS lab;

DROP TABLE IF EXISTS lab.events CASCADE;

CREATE TABLE lab.events (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL
);
