create procedure Stage.Populate_Dim_Date_Year
(
    @year smallint
)
as

    set nocount on;
    set datefirst 1;
    
    with 
    L0(c) as ( select 1 union all select 1 union all select 1),       
    L1(c) as ( select 1 from L0 as A cross join L0 as B cross join L0 as C ),
    L2(c) as ( select 1 from L1 as A cross join L1 as B ),
    Nums as
    ( 
        select row_number() over (order by c) as n
        from L2
    ),
    d0 as
    (
        select dateadd(day, n - 1, datefromparts(@year, 1, 1)) d
        from Nums
        where n <= 367 and dateadd(day, n - 1, datefromparts(@year, 1, 1)) < datefromparts(@year + 1, 1, 1)
    ),
    d1 as
    (
        select 
            d0.d, 
            datepart(year, d0.d) as Year,
            datepart(month, d0.d) as Month,
            datepart(day, d0.d) as Day,
            convert(nchar(4), datepart(year, d0.d)) as YearName,
            case when datepart(month, d0.d) <= 3 then 0 else 1 end as FinancialYearOffset
        from d0
    ),
    d2 as
    (
        select 
            d1.*,
            Year + FinancialYearOffset as FinancialYear,
            dateadd(day, 1 - datepart(weekday, d1.d), d1.d) as FirstDayOfWeek,
            dateadd(day, 7 - datepart(weekday, d1.d), d1.d) as LastDayOfWeek,
            datefromparts(d1.Year, d1.Month, 1) as FirstDayOfMonth,
            eomonth(d1.d) as LastDayOfMonth,
            datefromparts(d1.Year, 1, 1) as FirstDayOfYear,
            datefromparts(d1.Year, 12, 31) as LastDayOfYear,
            datefromparts(d1.Year + FinancialYearOffset - 1, 4, 1) as FirstDayOfFinancialYear,
            datefromparts(d1.Year + FinancialYearOffset, 3, 31) as LastDayOfFinancialYear
        from d1
    )
    insert into Dim.Date
    (
        DateKey,
        DateTime,
        
        -- SCD 1
        LongDateDescription,
        ShortDateDescription,
        DateLabel,
        DateNumberInYear,
        
        Weekday,
        WeekdayName,
    
        DayNumberInMonth,
        MonthNumber,
        MonthName,
        MonthYear,
        MonthYearName,
    
        Quarter,
        QuarterName,
        QuarterYear,
        QuarterYearName,
        Year,
    
        FinancialQuarter,
        FinancialQuarterName,
        FinancialQuarterYear,
        FinancialQuarterYearName,
        FinancialYear,
        FinancialYearName,
    
        IsFirstDayOfWeek,
        IsLastDayOfWeek,
        FirstDayOfWeek,
        LastDayOfWeek,
    
        IsFirstDayOfMonth,
        IsLastDayOfMonth,
        FirstDayOfMonth,
        LastDayOfMonth,
    
        IsFirstDayOfYear,
        IsLastDayOfYear,
        FirstDayOfYear,
        LastDayOfYear,
    
        IsFirstDayOfFinancialYear,
        IsLastDayOfFinancialYear,
        FirstDayOfFinancialYear,
        LastDayOfFinancialYear
    )
    select
        datediff(day, '2016-01-01', d) as DateKey,
        d as Date,
        
        datename(weekday, d) + ', ' + convert(nvarchar, d, 106) as LongDateDescription,
        convert(nvarchar, d, 103) as ShortDateDescription,
        convert(nchar(8), d, 112) as DateLabel,
        datename(dayofyear, d) as DateNumberInYear,
    
        datepart(weekday, d) as Weekday,
        datename(weekday, d) as WeekdayName,
    
        Day as DayNumberInMonth,
        Month as MonthNumber,
        datename(month, d) as MonthName,
        (Year - 2000) * 100 + Month as MonthYear,
        datename(month, d) + ' ' + YearName as MonthYearName,
    
        case 
            when Month <= 3 then 1
            when Month <= 6 then 2
            when Month <= 9 then 3
            else 4
        end as Quarter,
        case 
            when Month <= 3 then 'Q1'
            when Month <= 6 then 'Q2'
            when Month <= 9 then 'Q3'
            else 'Q4'
        end as QuarterName,
        Year * 10 + case 
            when Month <= 3 then 1
            when Month <= 6 then 2
            when Month <= 9 then 3
            else 4
        end as QuarterYear,
        case 
            when Month <= 3 then 'Q1 '
            when Month <= 6 then 'Q2 '
            when Month <= 9 then 'Q3 '
            else 'Q4 '
        end + YearName as QuarterYearName,
        Year,
    
        case 
            when Month <= 3 then 4
            when Month <= 6 then 1
            when Month <= 9 then 2
            else 3
        end as FinancialQuarter,
        case 
            when Month <= 3 then 'Q4'
            when Month <= 6 then 'Q1'
            when Month <= 9 then 'Q2'
            else 'Q3'
        end as FinancialQuarterName,
        (Year + FinancialYearOffset) * 10 +
        case 
            when Month <= 3 then 4
            when Month <= 6 then 1
            when Month <= 9 then 2
            else 3
        end as FinancialQuarterYear,
        case 
            when Month <= 3 then 'Q4 FY' 
            when Month <= 6 then 'Q1 FY'
            when Month <= 9 then 'Q2 FY'
            else 'Q3 FY'
        end + convert(nchar(4), Year + FinancialYearOffset) as FinancialQuarterYearName,
        Year + FinancialYearOffset as FinancialYear,
        convert(nchar(4), Year + FinancialYearOffset) as FinancialYearName,
    
        case when d = FirstDayOfWeek then 1 else 0 end as IsFirstDayOfWeek,
        case when d = LastDayOfWeek then 1 else 0 end as IsLastDayOfWeek,
        FirstDayOfWeek,
        LastDayOfWeek,
    
        case when d = FirstDayOfMonth then 1 else 0 end as IsFirstDayOfMonth,
        case when d = LastDayOfMonth then 1 else 0 end as IsLastDayOfMonth,
        FirstDayOfMonth,
        LastDayOfMonth,
    
        case when d = FirstDayOfYear then 1 else 0 end as IsFirstDayOfYear,
        case when d = LastDayOfYear then 1 else 0 end as IsLastDayOfYear,
        FirstDayOfYear,
        LastDayOfYear,
    
        case when d = FirstDayOfFinancialYear then 1 else 0 end as IsFirstDayOfFinancialYear,
        case when d = LastDayOfFinancialYear then 1 else 0 end as IsLastDayOfFinancialYear,
        FirstDayOfFinancialYear,
        LastDayOfFinancialYear
    
    from d2;
return 0
