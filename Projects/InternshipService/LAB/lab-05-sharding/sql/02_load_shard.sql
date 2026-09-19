-- ============================================================
-- Лабораторная №5. Шаг 4: загрузка 100 000 заявок.
-- Router (hash(candidate_id) % N) распределяет строки по шардам:
-- каждая строка вставляется только на «свой» шард.
-- Выполняется на каждом шарде со своим индексом:
--   docker exec -i lab5-sharding-shard0-1 psql -U postgres -d postgres -v shard=0 < 02_load_shard.sql
--   docker exec -i lab5-sharding-shard1-1 psql -U postgres -d postgres -v shard=1 < 02_load_shard.sql
--   docker exec -i lab5-sharding-shard2-1 psql -U postgres -d postgres -v shard=2 < 02_load_shard.sql
--
-- hash = hashtext(candidate_id::text) & 2147483647  (неотрицательный int)
-- shard = hash % 3
-- ============================================================

INSERT INTO applications (id, candidate_id, vacancy_id, status, applied_date, cover_letter)
SELECT
    g,
    g,                                -- shard key: candidate_id = g (100 000 разных кандидатов)
    (g % 100) + 1,                    -- vacancy_id: 100 вакансий
    (g % 4),                          -- status: NEW/REVIEWED/ACCEPTED/REJECTED
    now() - (g || ' minutes')::interval,
    'Application of candidate ' || g
FROM generate_series(1, 100000) AS g
WHERE (hashtext(g::text) & 2147483647) % 3 = :shard;

SELECT count(*) AS rows_loaded FROM applications;
