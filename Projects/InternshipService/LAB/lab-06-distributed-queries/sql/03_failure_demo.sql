-- ============================================================
-- Лабораторная №6. Отказ одного shard (shard2 остановлен).
-- Запросы к доступным шардам продолжают работать:
-- данные кандидатов, попавших на shard0/shard1, доступны.
-- ============================================================

SELECT count(*) AS rows_on_this_shard FROM applications;

SELECT id, candidate_id, status
FROM applications
ORDER BY applied_date DESC
LIMIT 3;
