create procedure Stage.GetProjectionsInput
	@stockKey		int,
	@startDateKey	int
as
    select
        t.StockKey,
        t.Price,
        t.Volume,
    
        (select top 1 
            Dim.Sentiment.SentimentKey 
        from 
             Fact.StockAnnouncement 
        inner join 
             Dim.Sentiment 
        on 
            Fact.StockAnnouncement.SentimentKey = Dim.Sentiment.SentimentKey 
        where 
                Fact.StockAnnouncement.StockKey = s.StockKey and 
                Fact.StockAnnouncement.DateKey <= t.DateKey 
        order by Fact.StockAnnouncement.DateKey asc) as LatestSentimentKey,
        
        d.DateKey,
        d.DateNumberInYear,
        d.Weekday,
        d.MonthNumber,
        d.Year,
        d.FinancialQuarter,
        d.IsFirstDayOfMonth,
        d.IsLastDayOfMonth,
        d.IsFirstDayOfFinancialYear,
        d.IsLastDayOfFinancialYear,
    
        td.TimeOfDayKey
        
    from 
        Fact.StockPrice t
    inner join Dim.Stock s
        on s.StockKey = t.StockKey
    inner join Dim.Date d
        on t.DateKey = d.DateKey
    inner join Dim.TimeOfDay td
        on td.TimeOfDayKey = t.TimeOfDayKey
    
    where 
        t.StockKey = @stockKey
    and
        d.DateKey >= @startDateKey
    and 
        s.IsBlackListed = 0
return 0
