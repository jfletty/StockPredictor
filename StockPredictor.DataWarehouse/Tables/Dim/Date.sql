create table Dim.Date
(
    DateKey     smallint not null,
    DateTime    date not null,

    -- SCD 1
    LongDateDescription     nvarchar(25) not null,
    ShortDateDescription    nvarchar(12) not null,
    DateLabel               nchar(8) not null,

    DateNumberInYear        smallint not null,

    -- Week starts on monday
    Weekday             tinyint not null,
    WeekdayName         nvarchar(10) not null,

    DayNumberInMonth    tinyint not null,
    MonthNumber         tinyint not null,
    MonthName           nvarchar(10) not null,
    MonthYear           nvarchar(15) not null,
    MonthYearName       nvarchar(15) not null,

    Quarter             tinyint not null,
    QuarterName         nchar(2) not null,
    QuarterYear         smallint not null,
    QuarterYearName     nchar(7) not null,
    Year                smallint not null,

    -- Financial
    FinancialQuarter            tinyint not null,
    FinancialQuarterName        nchar(2) not null,
    FinancialQuarterYear        smallint not null,
    FinancialQuarterYearName    nchar(9) not null,
    FinancialYear               smallint not null,
    FinancialYearName           nchar(7) not null,
    
    -- First and last
    IsFirstDayOfMonth   bit not null,
    IsLastDayOfMonth    bit not null,
    FirstDayOfMonth     date not null,
    LastDayOfMonth      date not null,

    IsFirstDayOfWeek    bit not null,
    IsLastDayOfWeek     bit not null,
    FirstDayOfWeek      date not null,
    LastDayOfWeek       date not null,

    IsFirstDayOfYear    bit not null,
    IsLastDayOfYear     bit not null,
    FirstDayOfYear      date not null,
    LastDayOfYear       date not null,

    IsFirstDayOfFinancialYear   bit not null,
    IsLastDayOfFinancialYear    bit not null,
    FirstDayOfFinancialYear     date not null,
    LastDayOfFinancialYear      date not null,

    constraint PK_Date primary key (DateKey)
)
go;

create index IX_Date_Date 
    on Dim.Date([DateTime])
go;
