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
    [ObservableProperty] private string _selectedPeriod = "Ez a hónap";

    public List<string> Periods { get; } = ["Ez a hónap", "Utolsó 3 hónap", "Ez az év", "Összes"];

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
            "Ez a hónap"      => (new DateTime(now.Year, now.Month, 1), now),
            "Utolsó 3 hónap"  => (now.AddMonths(-3), now),
            "Ez az év"        => (new DateTime(now.Year, 1, 1), now),
            _                 => (DateTime.MinValue, now)
        };
    }
}
