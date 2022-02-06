create table Fact.StockKeyDetails
(
	StockKey				int	not null,

	StaffCount				int null,
	CompanyAge				int null,

	AuditRisk				int null,
	BoardRisk				int null,
	CompensationRisk		int null,
	ShareHolderRightsRisk	int null,
	OverallRisk				int null,

	ValidFrom	datetime2 generated always as row start not null,
	ValidTo		datetime2 generated always as row end not null,
	period for system_time (ValidFrom, ValidTo),
	constraint PK_StockKeyDetails primary key (StockKey),
	constraint FK_StockKeyDetails_Stock foreign key (StockKey) references Dim.Stock(StockKey)
)
with (system_versioning = on (history_table = Fact.StockKeyDetailsHistory));
