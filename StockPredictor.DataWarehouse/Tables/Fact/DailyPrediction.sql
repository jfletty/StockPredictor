create table Fact.DailyPrediction
(
	DateKey					smallint not null,
	StockKey				int not null,
	GeneratedAtDateTime		datetime not null,
	PredictedClose			money not null,
	constraint PK_DailyPrediction primary key (StockKey, DateKey)
)
go;
