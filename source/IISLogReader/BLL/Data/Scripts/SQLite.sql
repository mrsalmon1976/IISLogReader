CREATE TABLE IF NOT EXISTS Projects (
	Id INTEGER PRIMARY KEY,
	Name text NOT NULL,
	CreateDate text NOT NULL
);

CREATE TABLE IF NOT EXISTS LogFiles (
	Id INTEGER PRIMARY KEY,
	ProjectId text NOT NULL,
	FileName text NOT NULL,
	FileHash text NOT NULL,
	CreateDate text NOT NULL,
	FileLength INTEGER NOT NULL,
	RecordCount INTEGER NOT NULL,
	Status INTEGER NOT NULL,
	ErrorMsg text NULL,
	FOREIGN KEY(ProjectId) REFERENCES Project(Id)
);

CREATE TABLE IF NOT EXISTS Requests (
	Id INTEGER PRIMARY KEY,
	LogFileId text NOT NULL,
	RequestDateTime text NULL,
	ClientIp text NULL,
	UserName text NULL,
	ServiceName text NULL,
	ServerName text NULL,
	ServerIp text NULL,
	ServerPort text NULL,
	Method text NULL,
	UriStem text NULL,
	UriStemAggregate text NULL,
	UriQuery text NULL,
	ProtocolStatus text NULL,
	BytesSent INTEGER NULL,
	BytesReceived INTEGER NULL,
	TimeTaken INTEGER NULL,
	ProtocolVersion text NULL,
	Host text NULL,
	UserAgent text NULL,
	Cookie text NULL,
	Referer text NULL,
	FOREIGN KEY(LogFileId) REFERENCES LogFiles(Id)
);

CREATE TABLE IF NOT EXISTS ProjectRequestAggregates (
	Id INTEGER PRIMARY KEY,
	ProjectId text NOT NULL,
	RegularExpression text NOT NULL,
	AggregateTarget text NULL,
	CreateDate text NOT NULL,
	FOREIGN KEY(ProjectId) REFERENCES Projects(Id)
);

CREATE TABLE IF NOT EXISTS Users (
	Id UNIQUEIDENTIFIER PRIMARY KEY,
	UserName text NOT NULL,
	Password text NOT NULL,
	Role text NULL
);
