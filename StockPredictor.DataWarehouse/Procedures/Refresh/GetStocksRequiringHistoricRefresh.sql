create procedure Stage.GetRequiringHistoricRefresh
as
with completeRefreshes as (
	select 
		ExternalKey
	from 
		Stage.Refresh 
	where
		Stage.Refresh.RefreshType = 4 and
		datediff(day, Stage.Refresh.StartDateTime, getuctdate()) < 7 and
		(Stage.Refresh.[Status] != 2 or Stage.Refresh.FailedAttempt != 5)
	)
	select 
		top(100)
		Stage.Symbol.SymbolValue + (case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end) as ExchangeKey
	from 
		Stage.Symbol 
	left join 
		completeRefreshes
		on 
			Stage.Symbol.SymbolValue + case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end = completeRefreshes.ExternalKey
	inner join 
		Dim.Stock
	on
		Dim.Stock.SymbolKey = Stage.Symbol.SymbolKey
	where 
		Stage.Symbol.IsDisabled = 0 and
		completeRefreshes.ExternalKey is null
RETURN 0
