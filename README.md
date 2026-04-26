# 💰 Personal Finance Tracker

A modern desktop application for tracking personal income and expenses, built with **WPF** and **.NET 10** following the **MVVM** pattern. Features include category management, interactive charts, multi-currency support with live exchange rates, CSV export, and AI-powered category suggestions via **Google Gemini**.

> Built as a portfolio project to demonstrate clean architecture, MVVM, EF Core, REST integration, and modern WPF UI design.

---

## 📑 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Class Reference](#-class-reference)
  - [Models](#models)
  - [Data Layer](#data-layer)
  - [Services](#services)
  - [ViewModels](#viewmodels)
  - [Views](#views)
  - [Themes](#themes)
  - [App Entry Point](#app-entry-point)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
  - [Gemini API Key Setup](#gemini-api-key-setup)
  - [Currency API](#currency-api)
- [How It Works](#-how-it-works)
- [Database](#-database)
- [Roadmap](#-roadmap)
- [License](#-license)

---

## ✨ Features

| Feature | Description |
|---|---|
| 💸 **Transaction tracking** | Add, view, and delete income/expense transactions with description, amount, date, category, and optional notes |
| 🏷️ **Custom categories** | Built-in categories (Salary, Groceries, Bills…) plus user-defined ones with emoji icons |
| 📊 **Interactive charts** | Pie chart of expenses by category and 6-month bar chart of income vs. expense (LiveCharts2) |
| 💱 **Multi-currency** | Switch between HUF, EUR, USD, GBP — all amounts re-converted live using real-time rates from the [Frankfurter API](https://www.frankfurter.app/) |
| 📥 **CSV export** | Export transactions to a CSV file in the currently selected display currency |
| ✨ **AI category suggestion** | Click a button and Google Gemini suggests the most appropriate category based on the transaction description |
| 🎨 **Modern UI** | Custom-styled controls, sidebar navigation, top bar, card-based dashboard, polished dark sidebar |
| 💾 **Local persistence** | SQLite database via Entity Framework Core with code-first migrations |

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 (`net10.0-windows`) |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Pattern** | MVVM with `CommunityToolkit.Mvvm` (source-generated `[ObservableProperty]`, `[RelayCommand]`) |
| **Database** | SQLite via `Microsoft.EntityFrameworkCore.Sqlite` 9.0.4 (LTS) |
| **Charts** | [LiveCharts2](https://livecharts.dev/) (`LiveChartsCore.SkiaSharpView.WPF`) |
| **CSV** | [CsvHelper](https://joshclose.github.io/CsvHelper/) |
| **DI Container** | `Microsoft.Extensions.DependencyInjection` |
| **HTTP / JSON** | `System.Net.Http` + `System.Text.Json` |
| **Currency rates** | [Frankfurter API](https://www.frankfurter.app/) (free, no API key required) |
| **AI** | [Google Gemini API](https://aistudio.google.com/) — model: `gemini-2.5-flash` |

### NuGet packages

```xml
<PackageReference Include="CommunityToolkit.Mvvm"                   Version="8.4.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite"    Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools"     Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design"    Version="9.0.4" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.7" />
<PackageReference Include="CsvHelper"                                Version="33.x" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF"         Version="2.x" />
```

---

## 🏛 Architecture

The application follows the **MVVM (Model-View-ViewModel)** pattern with **dependency injection** at the entry point:

```
┌──────────────────────────────────────────────────────────────┐
│                         App.xaml.cs                          │
│   Configures DI container → resolves MainViewModel → shows   │
└──────────────────────────────────────────────────────────────┘
                              │
            ┌─────────────────┼──────────────────┐
            ▼                 ▼                  ▼
       ┌────────┐       ┌──────────┐       ┌──────────┐
       │ Models │◄──────│   Data   │◄──────│ Services │
       │  POCO  │       │ EF Core  │       │ Currency │
       │        │       │ DbContext│       │ Csv, AI  │
       └────────┘       └──────────┘       └──────────┘
                              ▲                  ▲
                              │                  │
                       ┌──────┴──────────────────┘
                       │
                  ┌─────────────┐
                  │ ViewModels  │  ◄── injected services + db
                  │ Observable  │
                  │ Properties  │
                  └─────────────┘
                       ▲
                       │ DataContext binding
                       │
                  ┌─────────────┐
                  │   Views     │  XAML UserControls
                  │  (XAML)     │  bound via DataTemplates
                  └─────────────┘
```

**Key principles:**

- **Views** know nothing about the database or services — they only bind to ViewModel properties and commands.
- **ViewModels** hold state as `[ObservableProperty]` fields and expose actions as `[RelayCommand]` methods. They never reference any `System.Windows` UI types (with one minor exception: `MessageBox` for user-facing alerts).
- **Services** are stateless / singleton helpers that wrap external concerns (HTTP, file I/O).
- **DataTemplates in `MainWindow.xaml`** map each ViewModel type to its corresponding View, enabling automatic view-switching when `MainViewModel.CurrentPage` changes.

---

## 📁 Project Structure

```
PersonalFinanceTracker/
│
├── App.xaml                 # Application resources, theme dictionaries
├── App.xaml.cs              # DI container, startup logic
├── MainWindow.xaml          # Sidebar + top bar + content area shell
├── MainWindow.xaml.cs       # Code-behind (empty — pure XAML)
│
├── Models/                  # Plain C# entities
│   ├── Transaction.cs       # Income/expense record
│   ├── Category.cs          # Category with icon/color/type
│   └── CurrencyInfo.cs      # Currency code + symbol + display name
│
├── Data/                    # EF Core persistence
│   ├── AppDbContext.cs      # DbContext with seeded categories
│   ├── AppDbContextFactory.cs # Design-time factory for EF migrations
│   └── DbInitializer.cs     # Runs Migrate() on startup
│
├── Services/                # External integrations / business logic
│   ├── CurrencyService.cs   # Frankfurter API + conversion helpers
│   ├── CsvExportService.cs  # CsvHelper-based exporter
│   └── GeminiService.cs     # Google Gemini REST client
│
├── ViewModels/              # MVVM presentation layer
│   ├── MainViewModel.cs        # Root VM — navigation + currency
│   ├── DashboardViewModel.cs   # Summary cards + chart data
│   ├── TransactionsViewModel.cs # CRUD + AI suggest + CSV export
│   ├── CategoriesViewModel.cs  # Category CRUD + emoji picker list
│   └── TransactionRow.cs       # Per-row wrapper with formatted amount
│
├── Views/                   # XAML pages
│   ├── DashboardPage.xaml      # Summary cards + 2 charts
│   ├── TransactionsPage.xaml   # Form + DataGrid
│   └── CategoriesPage.xaml     # Form + DataGrid
│
├── Themes/                  # Reusable styles & colors
│   ├── Colors.xaml          # Color palette + brushes
│   └── Styles.xaml          # TextBox / ComboBox / Button / DataGrid
│
└── Migrations/              # EF Core auto-generated migrations
    ├── 20260425191121_InitialCreate.cs
    ├── 20260425192041_LocalizeCategorySeed.cs
    └── AppDbContextModelSnapshot.cs
```

---

## 📚 Class Reference

### Models

#### `Models/Transaction.cs`

Plain entity representing a single financial transaction. Mapped to the `Transactions` table.

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Primary key |
| `Amount` | `decimal` | Stored in **base currency (HUF)** — all conversions happen on display |
| `Type` | `TransactionType` | `Income` or `Expense` |
| `Description` | `string` | User-entered short text |
| `Date` | `DateTime` | Defaults to today |
| `Note` | `string?` | Optional longer note |
| `CategoryId` / `Category` | FK + nav | Linked category |

#### `Models/Category.cs`

Represents a transaction category with visual metadata.

| Property | Type | Purpose |
|---|---|---|
| `Id` | `int` | Primary key |
| `Name` | `string` | e.g. "Groceries" |
| `Icon` | `string` | Emoji shown in the UI (e.g. `🛒`) |
| `Color` | `string` | Hex color (e.g. `#FF5722`) — used for chart slices |
| `Type` | `CategoryType` | `Income`, `Expense`, or `Both` |
| `Transactions` | navigation | All linked transactions |

#### `Models/CurrencyInfo.cs`

Immutable `record` describing a currency option. `ToString()` returns `"EUR (€)"` format used directly in the dropdown.

| Property | Purpose |
|---|---|
| `Code` | ISO 4217 code (`HUF`, `EUR`, `USD`, `GBP`) |
| `Symbol` | Display symbol (`Ft`, `€`, `$`, `£`) |
| `Name` | Full name for tooltips |

---

### Data Layer

#### `Data/AppDbContext.cs`

The Entity Framework Core `DbContext`. Exposes:

- `DbSet<Transaction> Transactions`
- `DbSet<Category> Categories`

**Seeded data:** in `OnModelCreating`, 11 default categories are inserted (Salary, Freelance, Investment, Other Income, Groceries, Bills, Transport, Entertainment, Health, Clothing, Other Expense). Each has an emoji icon and color used by the dashboard pie chart.

#### `Data/AppDbContextFactory.cs`

Implements `IDesignTimeDbContextFactory<AppDbContext>` — required by `dotnet ef migrations add` to instantiate the DbContext **outside** the running app (where DI is unavailable). It builds a context with hardcoded `Data Source=finance.db`.

#### `Data/DbInitializer.cs`

Single static method `Initialize(AppDbContext)` that calls `Migrate()` to apply pending migrations on startup. The first run creates `finance.db` with all tables and seed data.

---

### Services

#### `Services/CurrencyService.cs` `[Singleton]`

Manages the user's selected display currency and live exchange rates. Inherits from `ObservableObject` so the UI re-binds when the currency changes.

**Key concept:** all `Transaction.Amount` values are stored in the **base currency (HUF)**. This service converts on display (`FromBase`) and on input (`ToBase`).

| Member | Description |
|---|---|
| `BaseCurrency` *(const)* | `"HUF"` — the canonical storage currency |
| `CurrentCurrency` `[ObservableProperty]` | The user-selected display currency |
| `AvailableCurrencies` | Read-only list: HUF, EUR, USD, GBP |
| `InitializeAsync()` | Fetches rates from `https://api.frankfurter.app/latest?from=HUF&to=EUR,USD,GBP`. On failure, falls back to hardcoded rates so the app still works offline. |
| `FromBase(decimal)` | Converts a stored HUF amount into the display currency |
| `ToBase(decimal)` | Converts user-entered display-currency input back into HUF for storage |
| `Format(decimal)` | Returns formatted string with symbol, e.g. `"1 250 €"` |

#### `Services/CsvExportService.cs` `[Singleton]`

Wraps **CsvHelper** to write transactions to disk.

- `ExportAsync(IEnumerable<Transaction>, string filePath)`
  - Maps each transaction into a private `TransactionCsvRow` DTO
  - Converts `Amount` to the **current display currency** (so the file is human-readable)
  - Uses `CultureInfo.InvariantCulture` and `,` delimiter for maximum interoperability

**Output columns:** `Date`, `Description`, `Category`, `Type`, `Amount`, `Currency`, `Note`

#### `Services/GeminiService.cs` `[Singleton]`

REST client for the Google Gemini API.

| Member | Description |
|---|---|
| `ApiKey` *(const)* | **Replace this** with your key (see [Gemini API Key Setup](#gemini-api-key-setup)) |
| `Model` *(const)* | `"gemini-2.5-flash"` — fast and inexpensive |
| `IsConfigured` | `true` if the placeholder has been replaced |
| `SuggestCategoryAsync(description, categoryNames)` | Sends a zero-temperature prompt asking Gemini to pick the best matching category. Returns the trimmed category name, or throws a clear `InvalidOperationException` on API errors / safety blocks |

The response JSON is parsed defensively with `TryGetProperty` so missing fields produce a meaningful error message instead of a generic `KeyNotFoundException`.

---

### ViewModels

#### `ViewModels/MainViewModel.cs`

Root view-model wired up in `App.xaml.cs`. Holds:

- `Currency` — exposed for the top-bar dropdown binding
- `DashboardVM` / `TransactionsVM` / `CategoriesVM` — child VMs constructed eagerly so navigation is instant
- `CurrentPage` `[ObservableProperty]` — drives the `ContentControl` in `MainWindow.xaml`. WPF picks the right View via the `DataTemplate`s.
- `ActiveMenu` — used to highlight the active tab and show its name in the top bar
- `NavigateCommand(string)` — switches `CurrentPage`

#### `ViewModels/DashboardViewModel.cs`

Computes summary totals and chart data.

| Property | Description |
|---|---|
| `TotalIncome`, `TotalExpense`, `Balance` | Decimals in base currency |
| `TotalIncomeFormatted`, `TotalExpenseFormatted`, `BalanceFormatted` | Display strings using the current currency |
| `SelectedPeriod` | One of `"This Month"`, `"Last 3 Months"`, `"This Year"`, `"All Time"` |
| `Periods` | List for the period dropdown |
| `ExpenseByCategory` | `ISeries[]` for the **pie chart** — one slice per category with its `Color` |
| `MonthlySeries`, `MonthlyXAxes`, `MonthlyYAxes` | Data for the **6-month income vs expense bar chart** |
| `LoadAsync()` | Re-queries the DB and rebuilds chart data |

Listens to `CurrencyService.PropertyChanged` so it re-formats summaries and rebuilds chart values when the user switches currency.

#### `ViewModels/TransactionsViewModel.cs`

Handles the Transactions page: list + form + actions.

| Member | Description |
|---|---|
| `Rows` | `ObservableCollection<TransactionRow>` — each row wraps a `Transaction` with currency-aware formatting |
| `Categories` | Available categories for the form's dropdown |
| Form fields: `NewDescription`, `NewAmount`, `NewDate`, `NewType`, `NewCategory`, `NewNote` | Bound to the input form |
| `IsSuggesting` | `true` while a Gemini call is in flight — disables the AI button |
| `AmountLabel` | `"Amount (€)"` etc. — updates with currency |
| `LoadAsync()` | Reloads transactions and categories |
| `AddTransactionCommand` | Validates inputs, converts amount **back to base currency** via `CurrencyService.ToBase`, saves |
| `DeleteTransactionCommand(TransactionRow)` | Deletes a row |
| `SuggestCategoryCommand` | Calls `GeminiService.SuggestCategoryAsync` and matches the returned name to one of the existing categories |
| `ExportCsvCommand` | Opens `SaveFileDialog`, calls `CsvExportService.ExportAsync` |

#### `ViewModels/TransactionRow.cs`

Lightweight per-row wrapper around a `Transaction` for the DataGrid. Exposes a `FormattedAmount` property that uses the current currency, and listens to `CurrencyService.PropertyChanged` to push updates to the UI without rebinding the whole grid.

#### `ViewModels/CategoriesViewModel.cs`

Manages the Categories page.

| Member | Description |
|---|---|
| `Categories` | `ObservableCollection<Category>` |
| Form fields: `NewName`, `NewIcon`, `NewType` | Bound to the form |
| `AvailableIcons` | Curated list of ~40 emoji used as the icon picker dropdown |
| `CategoryTypes` | `Income`, `Expense`, `Both` |
| `AddCategoryCommand` / `DeleteCategoryCommand` | CRUD operations |

---

### Views

All Views are `UserControl`s with **no code-behind logic** — purely XAML driven by their bound ViewModel.

#### `Views/DashboardPage.xaml`
- Header with period selector
- Three summary cards (Balance / Income / Expense)
- Two chart cards: pie + bar — both bind to `DashboardViewModel`'s chart properties

#### `Views/TransactionsPage.xaml`
- Header with **📥 Export CSV** button on the right
- Two-row form grid: labels in row 0, inputs in row 1, plus **✨ AI** and **+ Add** buttons
- DataGrid bound to `Rows`, with colored type/amount columns and a delete button per row

#### `Views/CategoriesPage.xaml`
- Header
- Two-row form grid with name input, **emoji dropdown** (rendered with `ItemTemplate`), type, and Add button
- DataGrid bound to `Categories` with delete button per row

#### `MainWindow.xaml` (shell)
- 220-px sidebar with logo (a `$` tile) and three navigation buttons
- Top bar (64 px) with the active page name and a global **currency selector** ComboBox
- `ContentControl` that auto-resolves the active ViewModel into its View via `DataTemplate`s

---

### Themes

#### `Themes/Colors.xaml`
Centralised color palette: `BackgroundColor`, `SidebarColor`, `CardColor`, `PrimaryColor`, `IncomeColor`, `ExpenseColor`, plus matching `SolidColorBrush` resources.

#### `Themes/Styles.xaml`
Reusable styles applied to controls:

| Style | Target | Notes |
|---|---|---|
| `CardStyle` | `Border` | White rounded card with subtle drop shadow |
| `FieldLabel` | `TextBlock` | Small grey form label |
| (default) | `TextBox` | 38px height, rounded, primary border on focus |
| (default) | `ComboBox` | Custom template matching TextBox visually, with a chevron arrow and rounded popup |
| `PrimaryButton` | `Button` | 38px height, primary blue, hover/press states |
| `DangerButton` | `Button` | 32px height, red — used for delete |
| `NavButton` | `Button` | Sidebar item with hover highlight |
| (default) | `DataGrid` | Single-select, no grid lines except horizontal |

Crucially, `TextBox`, `ComboBox`, and `PrimaryButton` all share the same `InputHeight` resource (`38`) — that's why form fields and the **+ Add** button line up perfectly.

---

### App Entry Point

#### `App.xaml.cs`

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var collection = new ServiceCollection();
    collection.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=finance.db"));
    collection.AddSingleton<CurrencyService>();
    collection.AddSingleton<CsvExportService>();
    collection.AddSingleton<GeminiService>();
    collection.AddSingleton<MainViewModel>();
    _services = collection.BuildServiceProvider();

    var db = _services.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(db);

    // Fetch live exchange rates in the background.
    var currency = _services.GetRequiredService<CurrencyService>();
    _ = currency.InitializeAsync();

    var window = new MainWindow
    {
        DataContext = _services.GetRequiredService<MainViewModel>()
    };
    window.Show();
}
```

This is the one place that wires everything together: DI registration, migrations, currency rate fetch, and showing the main window.

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11**
- **.NET 10 SDK** (or compatible) — `dotnet --version` should show `10.x`
- **Visual Studio 2022/2026** or **JetBrains Rider** (optional — `dotnet` CLI works too)
- A free **Google Gemini API key** if you want AI category suggestions ([get one here](https://aistudio.google.com/app/apikey))

### Clone and run

```bash
git clone https://github.com/Fejanos/PersonalFinanceTracker.git
cd PersonalFinanceTracker

# Restore dependencies and run
dotnet restore
dotnet build
dotnet run --project PersonalFinanceTracker
```

The first run creates `finance.db` (SQLite) in the project's output folder with the seeded categories.

### From Visual Studio

1. Open `PersonalFinanceTracker.slnx`
2. Set `PersonalFinanceTracker` as the startup project
3. Press <kbd>F5</kbd>

---

## ⚙️ Configuration

### Gemini API Key Setup

The AI category suggestion feature requires a Gemini API key.

1. Go to [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Sign in with your Google account and click **"Create API key"**
3. Copy the key (starts with `AIza…`)
4. Open the file:

   ```
   PersonalFinanceTracker/Services/GeminiService.cs
   ```

5. Replace the placeholder near the top:

   ```csharp
   // ===== HERE IS YOUR API KEY =====
   private const string ApiKey = "YOUR_GEMINI_API_KEY_HERE";
   ```

   …with your actual key:

   ```csharp
   private const string ApiKey = "AIzaSy...your_actual_key...";
   ```

6. Rebuild and run.

> ⚠️ **Security note:** committing your real API key to a public Git repository **exposes it**. Either don't commit it, or revoke and rotate the key. For a production-grade solution, move it into `appsettings.Local.json` (already in `.gitignore`) and load it via `IConfiguration`.

If the AI button is clicked while the placeholder is still in place, the app shows a friendly *"Gemini API key is not set"* message instead of failing.

### Currency API

The Frankfurter API is **free and requires no API key**. On first launch the app fetches today's rates; if the call fails (offline, network error, etc.) the app falls back to hardcoded rates so it remains usable.

---

## 🔄 How It Works

### Adding a transaction

1. User types description, amount, picks type and category
2. Optional: clicks **✨ AI** → `TransactionsViewModel.SuggestCategoryCommand` calls `GeminiService.SuggestCategoryAsync` with the description and the list of category names
3. Gemini returns a category name → matched against the local `Categories` collection → dropdown updated
4. User clicks **+ Add** → `AddTransactionCommand` runs:
   - Validates inputs
   - Converts the amount from the **display currency back to HUF** via `CurrencyService.ToBase`
   - Saves to SQLite via EF Core
5. Grid is reloaded and dashboard summaries refresh on next visit

### Switching currency

1. User picks a new currency from the top-bar dropdown
2. `CurrencyService.CurrentCurrency` changes → fires `PropertyChanged`
3. `DashboardViewModel`, every `TransactionRow`, and `TransactionsViewModel.AmountLabel` re-render their formatted strings instantly
4. `DashboardViewModel.LoadAsync()` re-runs to rebuild chart values in the new currency

### Exporting CSV

1. Click **📥 Export CSV** in the Transactions header
2. `SaveFileDialog` opens with a default filename like `transactions-2026-04-25.csv`
3. `CsvExportService` maps each transaction (converting amounts to the active display currency) and writes via `CsvHelper`
4. A confirmation dialog reports the number of rows written

---

## 💾 Database

- **Engine:** SQLite (file: `finance.db` in the working directory)
- **ORM:** Entity Framework Core 9 (LTS)
- **Migrations:** code-first, in the `Migrations/` folder
- **Seed data:** 11 default categories inserted by `OnModelCreating`

### Adding a new migration

```bash
cd PersonalFinanceTracker
dotnet ef migrations add YourMigrationName
dotnet ef database update    # optional — Migrate() runs on startup anyway
```

`AppDbContextFactory` is what makes `dotnet ef` able to construct the context outside the app's DI graph.

---

## 🗺 Roadmap

Possible next features (not yet implemented):

- [ ] **README screenshots** of the dashboard and forms
- [ ] **Edit transaction** (currently only add/delete)
- [ ] **Filter / search** the transactions grid (by date, category, type)
- [ ] **Dark mode** toggle
- [ ] **Recurring transactions** (monthly salary, subscriptions)
- [ ] **Per-category budgets** with over-spend warnings
- [ ] **Move API key to `appsettings.Local.json`** for safer config
- [ ] **Unit tests** for ViewModels and Services

---

## 📄 License

This project is licensed under the **MIT License** — see [`LICENSE`](LICENSE) for the full text.

```
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
```

### Third-party credits

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — MIT
- [Entity Framework Core](https://github.com/dotnet/efcore) — MIT
- [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) — MIT
- [CsvHelper](https://github.com/JoshClose/CsvHelper) — MS-PL / Apache 2.0 dual-licensed
- [Frankfurter API](https://www.frankfurter.app/) — open data, free for personal and commercial use
- [Google Gemini API](https://ai.google.dev/) — usage subject to Google's terms

---

## 👤 Author

Built by **Fejanos** as a portfolio piece showcasing modern WPF, MVVM, Entity Framework, and AI integration.
