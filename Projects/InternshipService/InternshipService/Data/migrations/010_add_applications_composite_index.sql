create index if not exists idx_applications_candidate_applied_date
    on applications (candidate_id, applied_date desc);
