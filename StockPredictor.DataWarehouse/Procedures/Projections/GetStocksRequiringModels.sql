create procedure Stage.GetStocksRequiringModels
as

with completedProjections as 
(
	select StockKey
	from 
		Stage.ProjectionSetting
	where
		UpdateType = 0 and
		datediff(day, Stage.ProjectionSetting.StartDateTime, getutcdate()) <= 7
)
select
		Dim.Stock.StockKey
	from
		Dim.Stock
	inner join 
		Stage.Symbol
	on
		Dim.Stock.SymbolKey = Stage.Symbol.SymbolKey
	left outer join
		completedProjections
	on
		completedProjections.StockKey = Dim.Stock.StockKey
	where
		Stage.Symbol.IsDisabled = 0 and
		completedProjections.StockKey is null

return 0
