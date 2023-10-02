create table History(
	Id integer primary key autoincrement not null,
	PluginId text not null collate nocase,
	ParentTypeId text null collate nocase,
	TypeId text not null collate nocase,
	Json text not null,
	LastAccess text not null collate nocase,
	AccessCount integer not null
)
