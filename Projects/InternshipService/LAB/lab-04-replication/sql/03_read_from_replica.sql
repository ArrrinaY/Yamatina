-- ============================================================
-- Лабораторная №4. Часть 3: доказательство репликации (запрос на Replica)
-- Запуск: docker exec -i internshipservice-postgres-replica-1 psql -U postgres -d postgres < 03_read_from_replica.sql
-- ============================================================

-- Replica работает в режиме восстановления (hot standby)
SELECT pg_is_in_recovery() AS is_replica;

-- Записи, созданные на Primary, видны на Replica
SELECT count(*) AS applications_on_replica FROM applications;

SELECT id, candidate_id, vacancy_id, status, applied_date
FROM applications
ORDER BY id DESC
LIMIT 5;
