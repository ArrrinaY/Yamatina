-- ============================================================
-- Лабораторная №4. Часть 6 (уточнённый замер lag): 
-- после INSERT мгновенно снимаем 10 выборок отставания Replica
-- по WAL (pg_wal_lsn_diff). Выполняется на Primary.
-- ============================================================

INSERT INTO applications (candidate_id, vacancy_id, status, cover_letter)
VALUES (779, 9, 0, 'lag probe: watch replay_lag_bytes decrease');

SELECT clock_timestamp() AS sample_time,
       g AS sample_no,
       (SELECT pg_wal_lsn_diff(pg_current_wal_lsn(), replay_lsn)
        FROM pg_stat_replication) AS replay_lag_bytes,
       (SELECT replay_lag FROM pg_stat_replication) AS replay_lag
FROM generate_series(1, 10) AS g;
