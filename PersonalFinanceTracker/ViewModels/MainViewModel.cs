using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinanceTracker.Data;

namespace PersonalFinanceTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty]
    private ObservableObject? _currentPage;

    [ObservableProperty]
    private string _activeMenu = "Dashboard";

    public DashboardViewModel DashboardVM { get; }
    public TransactionsViewModel TransactionsVM { get; }
    public CategoriesViewModel CategoriesVM { get; }

    public MainViewModel(AppDbContext db)
    {
        _db = db;
        DashboardVM = new DashboardViewModel(db);
        TransactionsVM = new TransactionsViewModel(db);
        CategoriesVM = new CategoriesViewModel(db);
        CurrentPage = DashboardVM;
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
