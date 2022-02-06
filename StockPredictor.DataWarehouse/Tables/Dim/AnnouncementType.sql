create table Dim.AnnouncementType 
(
	AnnouncementTypeKey	smallint not null identity (1,1),
	Name				varchar(15) not null,
	Description         nvarchar(100) null,
	constraint PK_AnnouncementType primary key (AnnouncementTypeKey) 
)
go;

create unique index UX_AnnouncementType 
    on Dim.AnnouncementType (Name)
go;