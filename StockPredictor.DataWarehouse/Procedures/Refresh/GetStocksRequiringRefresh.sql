create procedure Stage.GetStocksRequiringRefresh
	@frequency	int
as
with completeRefreshes as (
		select 
			ExternalKey 
		from 
			Stage.Refresh 
		where
			Stage.Refresh.RefreshType = 0 and 
			(datediff(minute, Stage.Refresh.EndDateTime, getutcdate()) <= @frequency or Stage.Refresh.Status = 2)
	)
	select 
		top(3000)
		Stage.Symbol.SymbolValue + (case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end) as ExchangeKey
	from 
		Stage.Symbol 
	left join 
		completeRefreshes
		on 
			Stage.Symbol.SymbolValue + case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end = completeRefreshes.ExternalKey
	where 
		(Stage.Symbol.IsDisabled = 0 or 
		datediff(day, Stage.Symbol.DisabledDate, getutcdate()) >= 30) and
		completeRefreshes.ExternalKey is null
RETURN 0
