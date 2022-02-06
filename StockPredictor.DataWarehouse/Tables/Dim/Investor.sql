create table Dim.Investor 
(
	InvestorKey		tinyint not null identity(1,1),
	InvestorName	nvarchar(100) not null,
	constraint PK_Investor primary key (InvestorKey)
)
go;

create index IX_Investor_InvestorKey
	on Dim.Investor(InvestorKey)
go;