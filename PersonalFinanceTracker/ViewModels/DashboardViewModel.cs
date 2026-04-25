using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpense;
    [ObservableProperty] private decimal _balance;
    [ObservableProperty] private string _selectedPeriod = "This Month";

    public List<string> Periods { get; } = ["This Month", "Last 3 Months", "This Year", "All Time"];

    public DashboardViewModel(AppDbContext db)
    {
        _db = db;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var (from, to) = GetPeriodRange();

        var transactions = await _db.Transactions
            .Where(t => t.Date >= from && t.Date <= to)
            .ToListAsync();

        TotalIncome  = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        Balance      = TotalIncome - TotalExpense;
    }

    partial void OnSelectedPeriodChanged(string value) => _ = LoadAsync();

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
