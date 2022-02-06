create view Fact.DailyActualVsPredictedView
as
select 
    Fact.DailyPrediction.StockKey,
    Fact.DailyPrediction.DateKey,
    Fact.DailyPrediction.PredictedClose,
    Fact.DailyPriceRecap.RegularMarketClose as ActualClose,
    abs(Fact.DailyPrediction.PredictedClose - Fact.DailyPriceRecap.RegularMarketClose) as AbsoluteDifference,
    Fact.DailyPrediction.PredictedClose - Fact.DailyPriceRecap.RegularMarketClose as Difference
from
    Fact.DailyPrediction
inner join 
    Fact.DailyPriceRecap
on 
    Fact.DailyPrediction.DateKey = Fact.DailyPriceRecap.DateKey and 
    Fact.DailyPrediction.StockKey = Fact.DailyPriceRecap.StockKey