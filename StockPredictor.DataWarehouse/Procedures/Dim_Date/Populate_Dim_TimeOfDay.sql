create procedure Stage.Populate_Dim_TimeOfDay
as

    if not exists(select top 1 1 from Dim.TimeOfDay)
    begin
    
        with 
        L0(c) as ( select 1 union all select 1 ),       
        L1(c) as ( select 1 from L0 as A cross join L0 as B ),
        L2(c) as ( select 1 from L1 as A cross join L1 as B ),
        L3(c) as ( select 1 from L2 as A cross join L2 as B ),
        L4(c) as ( select 1 from L3 as A cross join L3 as B ),
        Nums as
        ( 
            select row_number() over (order by c) as n
            from L4
        ),
        t0 as
        (
            select n - 1 as n
            from Nums
            where n <= 1440
        ),
        t1 as 
        (
            select 
                n,
                convert(time(0), dateadd(minute, 1 * n, 0)) t,
                n / 60 as h
            from t0
        )
        insert into Dim.TimeOfDay
        (
            TimeOfDayKey,
            Time,
    
            Hour,
            HourOfDay,
            MinuteOfHour,
    
            MorningAfternoon,
            TimeOfDay12Hour,
            TimeOfDay24Hour
        )
        select
            n,
            t,
    
            convert(time, dateadd(hour, h, 0)),
            h,
            n * 1 - h * 60,
    
            case when h < 12 then N'am' else N'pm' end,
            convert(nchar(5), convert(time(0), dateadd(minute, 1 * n - case when h < 12 then 0 else 720 end, 0))) + case when h < 12 then ' am' else ' pm' end,
            convert(nchar(5), t)
    
        from t1
    end
return 0
