-- ============================================================
-- Лабораторная №4. Часть 2: состояние streaming replication на Primary
-- Запуск: docker exec -i internshipservice-postgres-1 psql -U postgres -d postgres < 02_replication_status.sql
-- ============================================================

-- Подключённая Replica (одна строка = один активный WAL-ресивер)
SELECT pid,
       usename,
       application_name,
       client_addr,
       state,
       sent_lsn,
       write_lsn,
       flush_lsn,
       replay_lsn,
       write_lag,
       flush_lag,
       replay_lag
FROM pg_stat_replication;

-- Текущая позиция WAL на Primary
SELECT pg_current_wal_lsn() AS primary_wal_lsn;

-- Активность WAL-генерации
SELECT pg_walfile_name(pg_current_wal_lsn()) AS current_wal_file;
