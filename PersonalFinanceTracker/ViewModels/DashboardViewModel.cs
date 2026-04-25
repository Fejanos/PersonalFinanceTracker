using System.ComponentModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using SkiaSharp;

namespace PersonalFinanceTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly CurrencyService _currency;

    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpense;
    [ObservableProperty] private decimal _balance;

    [ObservableProperty] private string _selectedPeriod = "This Month";

    [ObservableProperty] private ISeries[] _expenseByCategory = [];
    [ObservableProperty] private ISeries[] _monthlySeries = [];
    [ObservableProperty] private Axis[] _monthlyXAxes = [];
    [ObservableProperty] private Axis[] _monthlyYAxes = [];

    public List<string> Periods { get; } = ["This Month", "Last 3 Months", "This Year", "All Time"];

    public string TotalIncomeFormatted  => _currency.Format(TotalIncome);
    public string TotalExpenseFormatted => _currency.Format(TotalExpense);
    public string BalanceFormatted      => _currency.Format(Balance);

    public DashboardViewModel(AppDbContext db, CurrencyService currency)
    {
        _db = db;
        _currency = currency;
        _currency.PropertyChanged += OnCurrencyChanged;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var (from, to) = GetPeriodRange();

        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= from && t.Date <= to)
            .ToListAsync();

        TotalIncome  = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        Balance      = TotalIncome - TotalExpense;

        BuildExpenseChart(transactions);
        BuildMonthlyChart(transactions);
    }

    private void BuildExpenseChart(List<Transaction> transactions)
    {
        var groups = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total    = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        ExpenseByCategory = groups.Select(g => (ISeries)new PieSeries<double>
        {
            Values = [(double)_currency.FromBase(g.Total)],
            Name = $"{g.Category.Icon} {g.Category.Name}",
            Fill = new SolidColorPaint(SKColor.Parse(g.Category.Color)),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
        }).ToArray();
    }

    private void BuildMonthlyChart(List<Transaction> transactions)
    {
        var months = Enumerable.Range(0, 6)
            .Select(i => DateTime.Today.AddMonths(-5 + i))
            .Select(d => new DateTime(d.Year, d.Month, 1))
            .ToList();

        var incomeValues = months.Select(m =>
            (double)_currency.FromBase(transactions
                .Where(t => t.Type == TransactionType.Income && t.Date >= m && t.Date < m.AddMonths(1))
                .Sum(t => t.Amount))).ToArray();

        var expenseValues = months.Select(m =>
            (double)_currency.FromBase(transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date >= m && t.Date < m.AddMonths(1))
                .Sum(t => t.Amount))).ToArray();

        MonthlySeries =
        [
            new ColumnSeries<double>
            {
                Values = incomeValues,
                Name = "Income",
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50"))
            },
            new ColumnSeries<double>
            {
                Values = expenseValues,
                Name = "Expense",
                Fill = new SolidColorPaint(SKColor.Parse("#F44336"))
            }
        ];

        MonthlyXAxes =
        [
            new Axis
            {
                Labels = months.Select(m => m.ToString("MMM yyyy")).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280"))
            }
        ];

        MonthlyYAxes =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#F3F4F6"))
            }
        ];
    }

    partial void OnSelectedPeriodChanged(string value) => _ = LoadAsync();

    partial void OnTotalIncomeChanged(decimal value)
        => OnPropertyChanged(nameof(TotalIncomeFormatted));
    partial void OnTotalExpenseChanged(decimal value)
        => OnPropertyChanged(nameof(TotalExpenseFormatted));
    partial void OnBalanceChanged(decimal value)
        => OnPropertyChanged(nameof(BalanceFormatted));

    private void OnCurrencyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalIncomeFormatted));
        OnPropertyChanged(nameof(TotalExpenseFormatted));
        OnPropertyChanged(nameof(BalanceFormatted));
        _ = LoadAsync(); // rebuild charts with new currency values
    }

    private (DateTime from, DateTime to) GetPeriodRange()
    {
        var now = DateTime.Today;
        return SelectedPeriod switch
        {
            "This Month"     => (new DateTime(now.Year, now.Month, 1), now),
            "Last 3 Months"  => (now.AddMonths(-3), now),
            "This Year"      => (new DateTime(now.Year, 1, 1), now),
            _                => (DateTime.MinValue, now)
        };
    }
}
