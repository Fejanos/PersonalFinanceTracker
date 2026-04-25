using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels;

public partial class TransactionRow : ObservableObject, IDisposable
{
    private readonly CurrencyService _currency;

    public Transaction Transaction { get; }

    public TransactionRow(Transaction transaction, CurrencyService currency)
    {
        Transaction = transaction;
        _currency = currency;
        _currency.PropertyChanged += OnCurrencyChanged;
    }

    public DateTime Date              => Transaction.Date;
    public string Description         => Transaction.Description;
    public Category Category          => Transaction.Category;
    public TransactionType Type       => Transaction.Type;
    public decimal Amount             => Transaction.Amount;
    public string FormattedAmount     => _currency.Format(Transaction.Amount);

    private void OnCurrencyChanged(object? sender, PropertyChangedEventArgs e)
        => OnPropertyChanged(nameof(FormattedAmount));

    public void Dispose() => _currency.PropertyChanged -= OnCurrencyChanged;
}
