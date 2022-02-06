create table Dim.TimeOfDay
(
    TimeOfDayKey    smallint    not null,
    [Time]          time(0)     not null,

    Hour            time(0) not null,
    HourOfDay       tinyint not null,
    MinuteOfHour    tinyint not null,

    MorningAfternoon    nchar(2) not null,
    TimeOfDay12Hour     nchar(8) not null,
    TimeOfDay24Hour     nchar(5) not null,
    
    constraint PK_TimeOfDay primary key (TimeOfDayKey)
)
go;
