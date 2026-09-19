-- ============================================================
-- Лабораторная №4. Часть 4: проверка read-only поведения Replica
-- Запуск: docker exec -i internshipservice-postgres-replica-1 psql -U postgres -d postgres < 04_readonly_test.sql
-- Ожидаемо: INSERT упадёт с ошибкой cannot execute INSERT in a read-only transaction
-- ============================================================

INSERT INTO applications (candidate_id, vacancy_id, status, cover_letter)
VALUES (999, 999, 0, 'Attempted write on replica');

-- Эта строка не должна появиться на Replica
SELECT count(*) AS rows_with_candidate_999 FROM applications WHERE candidate_id = 999;
