create table Dim.Sentiment 
(
	SentimentKey    tinyint not null identity (1,1),
	SentimentValue	nvarchar(15) not null,
	constraint PK_Sentiment primary key (SentimentKey)
)
go;

create unique index UI_Centiment_SentimentValue 
    on Dim.Sentiment(SentimentValue)
go;