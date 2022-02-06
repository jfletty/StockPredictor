create procedure Stage.GetStocksRequiringPriceRefresh
	@frequency	int
as
with completedRefreshes as 
(
select 
			ExternalKey 
		from 
			Stage.Refresh 
		where
			Stage.Refresh.RefreshType = 1 and 
			(datediff(minute, Stage.Refresh.EndDateTime, getuctdate()) <= @frequency or Stage.Refresh.Status = 2)
)
select 
	top(3000)
	Stage.Symbol.SymbolValue + (case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end) as ExchangeKey
from 
	Stage.Symbol 
left join
	completedRefreshes
on 
	Stage.Symbol.SymbolValue + case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end = completedRefreshes.ExternalKey
inner join 
	Dim.Stock
on
	Dim.Stock.SymbolKey = Stage.Symbol.SymbolKey
where 
	(
		Stage.Symbol.IsDisabled = 0 or
        datediff(day, Stage.Symbol.DisabledDate, getuctdate()) >= 10
	) and 		completedRefreshes.ExternalKey is null
return 0
