create table Fact.StockPrice
(
	-- Cluster Keys
	StockKey int not null,
	DateKey smallint not null,
	TimeOfDayKey smallint not null,

	-- measures
	Price		money	not null,
	Volume		bigint	not null,

	ExternalKey nvarchar(255) not null,

	constraint PK_StockPrice primary key nonclustered (StockKey, DateKey, TimeOfDayKey),
	constraint FK_StockPrice_Stock foreign key (StockKey) references Dim.Stock(StockKey),
	constraint FK_StockPrice_Date foreign key (DateKey) references Dim.Date(DateKey),
	constraint FK_StockPrice_TimeOfDay foreign key (TimeOfDayKey) references Dim.TimeOfDay(TimeOfDayKey)
);

go;

create clustered index CI_StockPrice
    on Fact.StockPrice (StockKey, DateKey asc, TimeOfDayKey asc)
go;

create unique index UI_StockPrice_ExternalKey 
    on Fact.StockPrice(ExternalKey)
go;

create nonclustered index UI_StockPrice_StockKeyDateKey 
    on Fact.StockPrice(StockKey, DateKey)
go;