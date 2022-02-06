using System.Collections.Generic;
using System.Threading.Tasks;
using StockPredictor.Algorithm.Domain.Tables.StoredProcedure;
using StockPredictor.Algorithm.Services.CsvMapping;

namespace StockPredictor.Algorithm.Services.Preparation
{
    public class PredictionCalculator
    {
        private readonly DatabaseRetriever _dbRetriever;

        public PredictionCalculator(DatabaseRetriever dbRetriever)
        {
            _dbRetriever = dbRetriever;
        }

        public async Task<Dictionary<int, string>> GenerateMockData(IEnumerable<StocksRequiringProjections> stocksRequiringProjections)
        {
            var result = new Dictionary<int, string>();
            var dates = await _dbRetriever.GetFutureDates(120);
            
            foreach (var stocks in stocksRequiringProjections)
            {
                var input = new List<DailyProjectionInput>();
                foreach (var date in dates)
                {
                    input.Add(new DailyProjectionInput
                    {
                        StockKey = stocks.StockKey,
                        RegularMarketClose = 0,
                        DateKey = date.DateKey,
                        DateNumberInYear = date.DateNumberInYear,
                        WeekDay = date.Weekday,
                        DayNumberInMonth = date.DayNumberInMonth,
                        MonthNumber = date.MonthNumber,
                        Year = date.Year,
                        Quarter = date.Quarter,
                        FinancialQuarter = date.FinancialQuarter,
                        IsFirstDayOfMonth = date.IsFirstDayOfMonth,
                        IsLastDayOfMonth = date.IsLastDayOfMonth,
                        IsFirstDayOfFinancialYear = date.IsFirstDayOfFinancialYear,
                        IsLastDayOfFinancialYear = date.IsLastDayOfFinancialYear,
                        IsFirstDayOfWeek = date.IsFirstDayOfWeek
                    });
                }
             
                result.Add(stocks.StockKey, Csv.SerializeToString(input));
            }
            return result;
        }
    }
}
