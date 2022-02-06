create procedure Stage.GetDailyModelInput
	@stockKey   int
as
	select
		p.StockKey,
		isnull(p.PreMarketChange, -99999) as PreMarketChange,
        isnull(p.PostMarketChange, -99999) as PostMarketChange,
        isnull(p.RegularMarketClose, 0) as RegularMarketClose , --predirector

        isnull(p.RegularMarketHigh - p.RegularMarketLow, -99999) as RegularMarketHighLowChange,
        isnull(p.RegularMarketHigh, -99999) as RegularMarketHigh,
        isnull(p.RegularMarketLow, -99999) as RegularMarketLow,
        isnull(p.RegularMarketVolume, -99999) as RegularMarketVolume,
        isnull(p.RegularMarketClose - p.RegularMarketOpen, -99999) as RegularMarketDayChange,
        isnull(p.MarketCap, -99999) as MarketCap,
		d.DateKey,
		d.DateNumberInYear,
		d.Weekday,
		d.DayNumberInMonth,
		d.MonthNumber,
		d.Year,
		d.Quarter,
		d.FinancialQuarter,
		d.IsFirstDayOfMonth,
		d.IsLastDayOfMonth,
		d.IsFirstDayOfFinancialYear,
		d.IsLastDayOfFinancialYear,
		d.IsFirstDayOfWeek		
	from 
		Fact.DailyPriceRecap p
	inner join Dim.Stock s
		on s.StockKey = p.StockKey
	inner join Dim.Date d
		on p.DateKey = d.DateKey
	where 
		p.StockKey = @stockKey
return 0
