namespace PersonalFinanceTracker.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Note { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

public enum TransactionType
{
    Income,
    Expense
}
