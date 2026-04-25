namespace PersonalFinanceTracker.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public string Color { get; set; } = "#607D8B";
    public CategoryType Type { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];
}

public enum CategoryType
{
    Income,
    Expense,
    Both
}
