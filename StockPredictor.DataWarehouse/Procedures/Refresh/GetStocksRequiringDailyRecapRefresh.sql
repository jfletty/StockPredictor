create procedure Stage.GetStocksRequiringDailyRecapRefresh
as
	with completeRefreshes as (
		select 
			ExternalKey
		from 
			Stage.Refresh 
		where 
			datediff(minute, Stage.Refresh.StartDateTime, getuctdate()) < 180 and
			Stage.Refresh.RefreshType = 3 and
			(Stage.Refresh.[Status] != 2 or Stage.Refresh.FailedAttempt != 5)
	)
	select
		top(1000)
			Stage.Symbol.SymbolValue + (case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end) as ExchangeKey
	from
		Dim.Stock
	inner join 
		Stage.Symbol
	on
		Dim.Stock.SymbolKey = Stage.Symbol.SymbolKey
	left outer join
		completeRefreshes
	on
		completeRefreshes.ExternalKey = Stage.Symbol.SymbolValue + (case when Stage.Symbol.ExchangeValue is null then '' else '.' + Stage.Symbol.ExchangeValue end)
	where
		Stage.Symbol.IsDisabled = 0 and
		completeRefreshes.ExternalKey is null
