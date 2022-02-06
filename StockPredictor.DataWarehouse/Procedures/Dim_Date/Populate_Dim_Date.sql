create procedure Stage.Populate_Dim_Date
(
    @now datetime2(0)
)
as
    set nocount on;
    set datefirst 1;
    
    declare 
        @date date = convert(date, @now),
        @periodDate date = convert(date, case when datepart(hour, @now) < 5 then dateadd(day, -1, @now) else @now end),
        @year smallint = 2010,
        @toYear smallint = datepart(year, @now) + 1;
    
    while @year <= @toYear
    begin
        if not exists (select top 1 1 from Dim.Date where Year = @year)
        begin
            exec Stage.Populate_Dim_Date_Year @year;
            exec Stage.Populate_Dim_TimeOfDay;
        end
    
        set @year = @year + 1;
    end

return 0
