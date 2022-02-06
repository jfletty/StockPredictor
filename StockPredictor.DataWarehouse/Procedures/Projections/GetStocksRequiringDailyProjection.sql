create procedure Stage.GetStocksRequiringDailyProjection
as
with completedProjections as 
(
	select StockKey
	from 
		Stage.ProjectionSetting
	where
		UpdateType = 1 and
		datediff(day, Stage.ProjectionSetting.StartDateTime, getutcdate()) <= 7
),
completedModels as 
(
	select distinct(StockKey) as StockKey
	from 
		Stage.ProjectionSetting
	where
		UpdateType = 0 and
		Status = 1 and
		datediff(day, Stage.ProjectionSetting.StartDateTime, getutcdate()) <= 1
)
select
		Dim.Stock.StockKey
	from
		Dim.Stock
	inner join 
		Stage.Symbol
	on
		Dim.Stock.SymbolKey = Stage.Symbol.SymbolKey
	inner join
		completedModels
	on
		completedModels.StockKey = Dim.Stock.StockKey
	left outer join
		completedProjections
	on
		completedProjections.StockKey = Dim.Stock.StockKey
	where
		Stage.Symbol.IsDisabled = 0 and
		completedProjections.StockKey is null

return 0
