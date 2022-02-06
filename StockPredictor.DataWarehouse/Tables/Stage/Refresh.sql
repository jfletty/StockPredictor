create table Stage.Refresh (
    ExternalKey   nvarchar (255)     not null,
    Status        int                not null,
    RefreshType   int                not null,
    FailedMessage nvarchar (255)     null,
    StartDateTime datetimeoffset (7) not null,
    EndDateTime   datetimeoffset (7) null,
    FailedAttempt int                default (0) not null,
	constraint PK_Refresh primary key nonclustered (ExternalKey, RefreshType)
)
go;

create nonclustered index CI_Refresh_RefreshTypeStatus 
    on Stage.Refresh (RefreshType, Status)
go;

create nonclustered index CI_Refresh_RefreshType 
    on Stage.Refresh (RefreshType) include (ExternalKey, EndDateTime)
go;