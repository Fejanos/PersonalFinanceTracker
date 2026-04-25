namespace PersonalFinanceTracker.Models;

public record CurrencyInfo(string Code, string Symbol, string Name)
{
    public override string ToString() => $"{Code} ({Symbol})";
}
