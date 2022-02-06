create table Fact.DailyPriceRecap
(
	StockKey int not null,
	DateKey smallint not null,

	PreMarketChange		    money null,
	PreMarketChangePercent  decimal(8,5) null,
	
	PostMarketChange	    money null,
	PostMarketChangePercent	decimal (8,5) null,

	RegularMarketPrice		money null,
	RegularMarketLow		money null,
	RegularMarketHigh		money null,

	RegularMarketVolume		bigint null,

	RegularMarketOpen		money null,
	RegularMarketClose		money null,

	MarketCap				bigint null,

	constraint PK_DailyPriceRecap primary key nonclustered (StockKey, DateKey),
	constraint FK_DailyPriceRecap_Stock foreign key (StockKey) references Dim.Stock(StockKey),
	constraint FK_DailyPriceRecap_Date foreign key (DateKey) references Dim.Date(DateKey)
)
go;
