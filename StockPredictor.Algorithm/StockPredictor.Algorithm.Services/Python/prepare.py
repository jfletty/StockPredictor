
def input_df_x(df):
    return df[['PreMarketChange', 'PostMarketChange', 'RegularMarketHighLowChange',
       'RegularMarketDayChange', 'MarketCap',
       'DateNumberInYear', 'WeekDay', 'DayNumberInMonth',
       'MonthNumber', 'Year', 'Quarter', 'FinancialQuarter',
       'IsFirstDayOfMonth', 'IsLastDayOfMonth',
       'IsFirstDayOfFinancialYear', 'IsLastDayOfFinancialYear',
       'IsFirstDayOfWeek']];