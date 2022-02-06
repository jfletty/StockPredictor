create table Fact.Prediction
(
	DateKey					int not null,
	TimeOfDayKey			smallint not null,
	StockKey				int not null,
	
	GeneratedAtDateTime		datetime not null,
	PredictedPrice			money not null,
	constraint PK_Prediction primary key (StockKey, DateKey, TimeOfDayKey)
)
go;
