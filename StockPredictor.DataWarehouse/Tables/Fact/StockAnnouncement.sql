create table Fact.StockAnnouncement
(
	StockAnnouncementKey	int not null,
	StockKey				int not null,
	AnnouncementTypeKey		smallint not null,
	SentimentKey			tinyint null,

	Title					nvarchar(255) not null,
	Uri						nvarchar(150) not null,

	-- measures
	DateKey					int not null,
	TimeOfDayKey			smallint not null,
	RelatedToDirector		bit not null default(0),
	RelatedToCompany		bit not null default(1),
	RelatedToRegion			bit not null default(0),
	RelatedToCountry		bit not null default(0),
	RelatedToWorld			bit not null default(0),

    constraint PK_StockAnnouncement primary key (StockAnnouncementKey)
)
go;

create unique index UI_StockAnnoucement_Uri 
    on Fact.StockAnnouncement(Uri)
go;