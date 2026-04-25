using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newIcon = "📁";
    [ObservableProperty] private string _newColor = "#607D8B";
    [ObservableProperty] private CategoryType _newType = CategoryType.Expense;

    public List<CategoryType> CategoryTypes { get; } = [CategoryType.Income, CategoryType.Expense, CategoryType.Both];

    public CategoriesViewModel(AppDbContext db)
    {
        _db = db;
        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        _db.Categories.Add(new Category
        {
            Name  = NewName,
            Icon  = NewIcon,
            Color = NewColor,
            Type  = NewType
        });

        await _db.SaveChangesAsync();
        NewName = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category? category)
    {
        if (category is null) return;
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }
}
