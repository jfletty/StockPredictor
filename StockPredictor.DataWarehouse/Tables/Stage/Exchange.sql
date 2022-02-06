create table Stage.Exchange
(
	ExchangeKey			varchar(5) not null,
	TradingStart		time null,
    TradingEnd			time null, 
    TimeZone            nvarchar(150) null,
    constraint PK_Exchange primary key (ExchangeKey)
)
go;
