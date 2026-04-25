using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<Transaction> _transactions = [];
    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private Transaction? _selectedTransaction;

    // New transaction form fields
    [ObservableProperty] private decimal _newAmount;
    [ObservableProperty] private string _newDescription = string.Empty;
    [ObservableProperty] private DateTime _newDate = DateTime.Today;
    [ObservableProperty] private TransactionType _newType = TransactionType.Expense;
    [ObservableProperty] private Category? _newCategory;
    [ObservableProperty] private string? _newNote;
    [ObservableProperty] private string _filterText = string.Empty;

    public List<TransactionType> TransactionTypes { get; } = [TransactionType.Income, TransactionType.Expense];

    public TransactionsViewModel(AppDbContext db)
    {
        _db = db;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

        Transactions = new ObservableCollection<Transaction>(transactions);
        Categories   = new ObservableCollection<Category>(categories);
        NewCategory  = Categories.FirstOrDefault();
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        if (NewCategory is null || NewAmount <= 0 || string.IsNullOrWhiteSpace(NewDescription))
            return;

        var transaction = new Transaction
        {
            Amount      = NewAmount,
            Type        = NewType,
            Description = NewDescription,
            Date        = NewDate,
            Note        = NewNote,
            CategoryId  = NewCategory.Id
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        NewAmount      = 0;
        NewDescription = string.Empty;
        NewNote        = null;
        NewDate        = DateTime.Today;

        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(Transaction? transaction)
    {
        if (transaction is null) return;
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }
}
