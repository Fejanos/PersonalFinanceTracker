using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly AppDbContext _db;
    private readonly CurrencyService _currency;
    private readonly CsvExportService _csv;
    private readonly GeminiService _gemini;

    [ObservableProperty] private ObservableCollection<TransactionRow> _rows = [];
    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private TransactionRow? _selectedRow;

    [ObservableProperty] private decimal _newAmount;
    [ObservableProperty] private string _newDescription = string.Empty;
    [ObservableProperty] private DateTime _newDate = DateTime.Today;
    [ObservableProperty] private TransactionType _newType = TransactionType.Expense;
    [ObservableProperty] private Category? _newCategory;
    [ObservableProperty] private string? _newNote;
    [ObservableProperty] private bool _isSuggesting;

    public List<TransactionType> TransactionTypes { get; } = [TransactionType.Income, TransactionType.Expense];

    public string AmountLabel => $"Amount ({_currency.CurrentCurrency.Symbol})";

    public TransactionsViewModel(AppDbContext db, CurrencyService currency, CsvExportService csv, GeminiService gemini)
    {
        _db = db;
        _currency = currency;
        _csv = csv;
        _gemini = gemini;
        _currency.PropertyChanged += OnCurrencyChanged;
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

        Rows = new ObservableCollection<TransactionRow>(
            transactions.Select(t => new TransactionRow(t, _currency)));
        Categories  = new ObservableCollection<Category>(categories);
        NewCategory = Categories.FirstOrDefault();
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        if (NewCategory is null || NewAmount <= 0 || string.IsNullOrWhiteSpace(NewDescription))
            return;

        // Convert from the user's display currency to the base currency for storage.
        var amountInBase = _currency.ToBase(NewAmount);

        _db.Transactions.Add(new Transaction
        {
            Amount      = amountInBase,
            Type        = NewType,
            Description = NewDescription,
            Date        = NewDate,
            Note        = NewNote,
            CategoryId  = NewCategory.Id
        });
        await _db.SaveChangesAsync();

        NewAmount      = 0;
        NewDescription = string.Empty;
        NewNote        = null;
        NewDate        = DateTime.Today;

        await LoadAsync();
    }

    [RelayCommand]
    private async Task SuggestCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDescription)) return;

        if (!_gemini.IsConfigured)
        {
            MessageBox.Show(
                "Gemini API key is not set. Open Services/GeminiService.cs and replace the placeholder.",
                "AI not configured",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            IsSuggesting = true;
            var suggestion = await _gemini.SuggestCategoryAsync(
                NewDescription,
                Categories.Select(c => c.Name));

            if (string.IsNullOrWhiteSpace(suggestion)) return;

            var match = Categories.FirstOrDefault(c =>
                string.Equals(c.Name, suggestion, StringComparison.OrdinalIgnoreCase));
            if (match is not null) NewCategory = match;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"AI suggestion failed:\n{ex.Message}",
                "AI error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        finally
        {
            IsSuggesting = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter   = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            FileName = $"transactions-{DateTime.Now:yyyy-MM-dd}.csv",
            Title    = "Export transactions"
        };
        if (dialog.ShowDialog() != true) return;

        var transactions = await _db.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        try
        {
            await _csv.ExportAsync(transactions, dialog.FileName);
            MessageBox.Show(
                $"Exported {transactions.Count} transactions to:\n{dialog.FileName}",
                "Export complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to export CSV:\n{ex.Message}",
                "Export error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(TransactionRow? row)
    {
        if (row is null) return;
        _db.Transactions.Remove(row.Transaction);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }

    private void OnCurrencyChanged(object? sender, PropertyChangedEventArgs e)
        => OnPropertyChanged(nameof(AmountLabel));
}
