create procedure Stage.Populate_Dim_Sentiment
as
	if not exists(select top 1 1 from Dim.Sentiment)
	begin
		insert into Dim.Sentiment (SentimentValue) values ('StrongNegative')
		insert into Dim.Sentiment (SentimentValue) values ('Negative')
		insert into Dim.Sentiment (SentimentValue) values ('Positive')
		insert into Dim.Sentiment (SentimentValue) values ('StrongPositive')
	end

return 0
