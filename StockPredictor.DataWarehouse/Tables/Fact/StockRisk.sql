create table Fact.StockRisk
(
	-- Cluster Keys
	StockKey				int not null,
	DateKey					smallint not null,
	TimeOfDayKey			tinyint not null,

	-- measures
	AuditRisk				int null,
	BoardRisk				int null,
	CompensationRisk		int null,
	ShareHolderRightsRisk	int null,
	OverallRisk				int null,

	[TimeStamp]		datetime not null,

	primary key nonclustered (StockKey, DateKey, TimeOfDayKey),
	constraint FK_TimeOfDay_TimeOfDayKey foreign key (TimeOfDayKey) references Dim.TimeOfDay(TimeOfDayKey),
	constraint FK_Date_DateKey foreign key (DateKey) references Dim.Date(DateKey)
);
go