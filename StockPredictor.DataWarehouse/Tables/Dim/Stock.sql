create table Dim.Stock
(
	StockKey			int not null identity(1,1),
	SymbolKey			int not null,
	LongName			nvarchar(255) null,
	Currency			nvarchar(100) null,

	StreetAddress		nvarchar(500) null,
	City				nvarchar(255) null,
	PostCode			nvarchar(100) null,
	Country				nvarchar(100) null,

	Website				nvarchar(155) null,
	IndustrySector		nvarchar(155) null,

	IsBlackListed		bit not null default(0),
	BlackListedDate		datetime null,

	constraint PK_Stock primary key (StockKey),
	constraint FK_Symbol_SymbolKey foreign key (SymbolKey) references Stage.Symbol(SymbolKey)
)
go

create index IX_Stock_SymbolKey 
    on Dim.Stock(SymbolKey);
go

create unique index _UX_Stock_SymbolKey 
    on Dim.Stock(SymbolKey)
go;
