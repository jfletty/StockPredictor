using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using StockPredictor.Algorithm.Domain;

namespace StockPredictor.Algorithm.Services.CsvMapping
{
    public class Csv
    {
        public static string SerializeToString<T>(IEnumerable<T> input)
        {
            using (var stringWriter = new StringWriter())
            using (var csv = new CsvWriter(stringWriter, CultureInfo.CurrentCulture, true))
            {
                csv.Configuration.ShouldQuote = (s, context) => true;
                csv.WriteRecords(input);
                return stringWriter.ToString();
            }
        }

        public static Result<IEnumerable<T>> DeserializeFromString<T>(IEnumerable<string> contents)
        {
            try
            {

                var result = new ConcurrentBag<T>();
                Parallel.ForEach(contents, content =>
                {
                    using (var stringReader = new StringReader(content))
                    using (var csv = new CsvReader(stringReader, CultureInfo.CurrentCulture))
                    {
                        var records = csv.GetRecords<T>();
                        Parallel.ForEach(records, record =>
                        {
                            result.Add(record);
                        });
                    }
                });

                return new Result<IEnumerable<T>>(result);
            }
            catch (HeaderValidationException e)
            {
                return new Result<IEnumerable<T>>(e);
            }
        }
    }
}
