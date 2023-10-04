create table History(
	Id integer primary key autoincrement not null,
	PluginId blob not null,
	ParentTypeId blob null,
	TypeId blob not null,
	Json text not null,
	LastAccess text not null collate nocase,
	AccessCount integer not null
);

create unique index "IX_History_(PluginId,TypeId,Json)" on History (PluginId, TypeId, Json);
