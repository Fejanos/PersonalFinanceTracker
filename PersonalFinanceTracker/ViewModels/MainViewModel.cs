using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentPage;

    [ObservableProperty]
    private string _activeMenu = "Dashboard";

    public CurrencyService Currency { get; }

    public DashboardViewModel DashboardVM { get; }
    public TransactionsViewModel TransactionsVM { get; }
    public CategoriesViewModel CategoriesVM { get; }

    public MainViewModel(AppDbContext db, CurrencyService currency, CsvExportService csv, GeminiService gemini)
    {
        Currency       = currency;
        DashboardVM    = new DashboardViewModel(db, currency);
        TransactionsVM = new TransactionsViewModel(db, currency, csv, gemini);
        CategoriesVM   = new CategoriesViewModel(db);
        CurrentPage    = DashboardVM;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        ActiveMenu = page;
        CurrentPage = page switch
        {
            "Dashboard"    => DashboardVM,
            "Transactions" => TransactionsVM,
            "Categories"   => CategoriesVM,
            _              => DashboardVM
        };
    }
}
