-- ============================================================
-- Лабораторная №6. Демонстрация запросов после шардирования.
-- Выполняется на шардах lab5 (см. LAB/lab-05-sharding).
-- ============================================================

-- 1. Single-shard query: WHERE candidate_id = 12345
-- Выполняем на всех трёх шардах — данные найдутся только на одном.
SELECT 'shard-?(candidate_id=12345)' AS probe, count(*) AS rows
FROM applications WHERE candidate_id = 12345;

-- 2. Агрегация по статусам: результат нужно собрать со всех шардов
SELECT status, count(*) AS cnt FROM applications GROUP BY status ORDER BY status;

-- 3. Общее количество заявок на шарде (части глобального COUNT)
SELECT count(*) AS shard_count FROM applications;

-- 4. ORDER BY + LIMIT: топ-10 самых свежих заявок НА ЭТОМ шарде
SELECT id, candidate_id, applied_date
FROM applications
ORDER BY applied_date DESC
LIMIT 10;
