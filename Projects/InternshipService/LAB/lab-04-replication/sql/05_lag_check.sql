-- ============================================================
-- Лабораторная №4. Часть 6: измерение replication lag
-- Скрипт выполняется на Primary сразу после записи и на Replica.
-- ============================================================

-- На Primary: позиция WAL после INSERT
-- (после выполнения INSERT из части 3/6)
SELECT pg_current_wal_lsn() AS primary_wal_lsn,
       pg_current_wal_insert_lsn() AS primary_wal_insert_lsn;

-- На Replica: до какой позиции WAL доиграны изменения
SELECT pg_last_wal_replay_lsn() AS replica_replay_lsn,
       pg_last_xact_replay_timestamp() AS last_replay_at,
       pg_is_in_recovery() AS is_replica;

-- Отставание Replica от Primary в байтах WAL
-- (вычисляется на Primary по pg_stat_replication)
SELECT client_addr,
       state,
       pg_wal_lsn_diff(pg_current_wal_lsn(), replay_lsn) AS replay_lag_bytes,
       replay_lag
FROM pg_stat_replication;
