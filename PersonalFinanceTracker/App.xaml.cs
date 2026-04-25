using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var collection = new ServiceCollection();
        collection.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite("Data Source=finance.db"));
        collection.AddSingleton<CurrencyService>();
        collection.AddSingleton<MainViewModel>();
        _services = collection.BuildServiceProvider();

        var db = _services.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(db);

        // Fetch live exchange rates in the background — UI uses fallback rates until it returns.
        var currency = _services.GetRequiredService<CurrencyService>();
        _ = currency.InitializeAsync();

        var window = new MainWindow
        {
            DataContext = _services.GetRequiredService<MainViewModel>()
        };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
