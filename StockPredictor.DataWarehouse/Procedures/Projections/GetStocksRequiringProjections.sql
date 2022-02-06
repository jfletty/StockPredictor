create procedure Stage.GetStocksRequiringProjections
as
declare @i int
declare @StockKey int
declare @numrows int
declare @Stock table (
    idx int primary key identity(1,1)
    , StockKey int
)
declare @DateByStock table (
    idx int primary key identity(1,1)
    , StockKey int
	, DateKey int
)
declare @ProjectionsByDateByStock table (
	DateKey int,
	StockKey int,
	GeneratedDateTime datetime
)
declare @StocksWithModels table(
	StockKey int
)

insert @Stock
    select distinct 
        StockKey 
    from 
         Dim.Stock 
    where 
        StockKey in (
            select 
                StockKey 
            from 
                Stage.ProjectionSetting 
            where 
                Status = 1 and 
                UpdateType = 0 
            group by StockKey)

insert @ProjectionsByDateByStock
    select 
        DateKey, 
        StockKey, 
        max(GeneratedAtDateTime) 
    from 
        Fact.Prediction 
    group by 
        DateKey, 
        StockKey

set @i = 1
set @numrows = (select count(*) from Dim.Stock)
IF @numrows > 0
    while (@i <= (select max(StockKey) from Dim.Stock))
    begin
        set @StockKey = (select StockKey from @Stock where idx = @i)

		insert into @DateByStock
			select 
				@StockKey,
				d.DateKey
		from 
			Dim.Date d
		where 
			d.DateKey not in (
				select 
					DateKey 
				from 
					@ProjectionsByDateByStock
				where 
					StockKey = @StockKey
					and DATEDIFF(Week, GeneratedDateTime, GETUTCDATE()) < 2
			) and
			d.DateTime <= dateadd(day, 90, getutcdate())

        set @i = @i + 1
    END

select 
    StockKey, 
    min(DateKey) as DateKey 
from 
    @DateByStock 
where 
    StockKey is not null 
group by 
    StockKey

return 0

