create table Cache(
	Key text primary key not null,
	Value text not null,
	Version integer not null,
	Updated text not null
)
