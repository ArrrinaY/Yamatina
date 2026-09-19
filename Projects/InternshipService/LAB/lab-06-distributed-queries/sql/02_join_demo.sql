-- ============================================================
-- Лабораторная №6. JOIN после шардирования.
-- Таблица vacancies (на которую ссылается applications.vacancy_id)
-- существует только на shard0 — имитация связанной таблицы,
-- не распределённой по тому же ключу.
-- ============================================================

-- Работает только там, где есть ОБЕ таблицы:
SELECT a.id, a.candidate_id, a.vacancy_id, v.title
FROM applications a
JOIN vacancies v ON a.vacancy_id = v.id
ORDER BY a.applied_date DESC
LIMIT 5;
