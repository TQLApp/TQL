create table Cache(
	Key text primary key not null collate nocase,
	Value text not null,
	Version integer not null,
	Updated text not null collate nocase
)
