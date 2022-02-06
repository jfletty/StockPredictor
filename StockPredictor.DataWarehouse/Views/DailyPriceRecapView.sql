create view Fact.DailyPriceRecapView
as
    select 
           * 
    from 
         Fact.DailyPriceRecap 
    where 
          DateKey >= 1850