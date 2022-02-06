create table Stage.ProjectionSetting
(
	ProjectionSettingKey	int not null identity(1,1),
	StockKey				int null,
	UpdateType				int not null default(1),
	Status					int	not null,
	FailedMessage			nvarchar(255) null,
	StartDateTime			DateTimeOffset(7) not null,
	EndDateTime				DateTimeOffset(7) null,

	constraint PK_ProjectionSetting primary key (ProjectionSettingKey)
)
go;
