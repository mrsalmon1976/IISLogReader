CREATE TABLE IF NOT EXISTS projects (
	id INTEGER PRIMARY KEY,
	project_name text NOT NULL,
	create_date text NOT NULL
);

CREATE TABLE IF NOT EXISTS log_files (
	id INTEGER PRIMARY KEY,
	project_id text NOT NULL,
	file_name text NOT NULL,
	file_hash text NOT NULL,
	create_date text NOT NULL,
	file_length INTEGER NOT NULL,
	record_count INTEGER NOT NULL,
	FOREIGN KEY(project_id) REFERENCES project(id)
);

CREATE TABLE IF NOT EXISTS requests (
	id INTEGER PRIMARY KEY,
	log_file_id text NOT NULL,
	date text NULL,
	time text NULL,
	client_ip text NULL,
	user_name text NULL,
	service_name text NULL,
	server_name text NULL,
	server_ip text NULL,
	server_port text NULL,
	method text NULL,
	uri_stem text NULL,
	uri_query text NULL,
	protocol_status text NULL,
	bytes_sent INTEGER NULL,
	bytes_received INTEGER NULL,
	time_taken INTEGER NULL,
	protocol_version text NULL,
	FOREIGN KEY(log_file_id) REFERENCES log_files(id)
);
