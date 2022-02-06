create table Dim.Director
(
	DirectorKey int not null identity(1,1),
	DirectorName nvarchar(100) not null,
	constraint PK_Director primary key (DirectorKey)
)
