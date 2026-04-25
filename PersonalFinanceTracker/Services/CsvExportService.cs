using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services;

public class CsvExportService(CurrencyService currency)
{
    public async Task ExportAsync(IEnumerable<Transaction> transactions, string filePath)
    {
        var rows = transactions.Select(t => new TransactionCsvRow
        {
            Date        = t.Date,
            Description = t.Description,
            Category    = t.Category.Name,
            Type        = t.Type.ToString(),
            Amount      = Math.Round(currency.FromBase(t.Amount), 2),
            Currency    = currency.CurrentCurrency.Code,
            Note        = t.Note ?? string.Empty
        });

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ","
        };

        await using var writer = new StreamWriter(filePath);
        await using var csv    = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(rows);
    }

    private sealed class TransactionCsvRow
    {
        public DateTime Date { get; set; }
        public string   Description { get; set; } = string.Empty;
        public string   Category { get; set; } = string.Empty;
        public string   Type { get; set; } = string.Empty;
        public decimal  Amount { get; set; }
        public string   Currency { get; set; } = string.Empty;
        public string   Note { get; set; } = string.Empty;
    }
}
