CREATE TABLE cases(
    case_id INTEGER PRIMARY KEY,
    case_number TEXT, 
    case_evidence_number TEXT,
    case_number_and_evidence_number TEXT,
    case_description TEXT,
    case_mirror_file_path_windows TEXT,
    case_mirror_file_path_linux TEXT,
    case_mirror_file_file TEXT,
    case_dir_path_windows TEXT,
    case_dir_path_linux TEXT,
    case_created_datetime TEXT,
    case_created_saying TEXT,
    case_updated_datetime TEXT,
    case_updated_saying TEXT
)