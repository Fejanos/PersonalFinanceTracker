using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1,  Name = "Salary",          Icon = "💼", Color = "#4CAF50", Type = CategoryType.Income },
            new Category { Id = 2,  Name = "Freelance",       Icon = "💻", Color = "#8BC34A", Type = CategoryType.Income },
            new Category { Id = 3,  Name = "Investment",      Icon = "📈", Color = "#009688", Type = CategoryType.Income },
            new Category { Id = 4,  Name = "Other Income",    Icon = "💰", Color = "#00BCD4", Type = CategoryType.Income },
            new Category { Id = 5,  Name = "Groceries",       Icon = "🛒", Color = "#FF5722", Type = CategoryType.Expense },
            new Category { Id = 6,  Name = "Bills",           Icon = "🏠", Color = "#795548", Type = CategoryType.Expense },
            new Category { Id = 7,  Name = "Transport",       Icon = "🚗", Color = "#607D8B", Type = CategoryType.Expense },
            new Category { Id = 8,  Name = "Entertainment",   Icon = "🎬", Color = "#9C27B0", Type = CategoryType.Expense },
            new Category { Id = 9,  Name = "Health",          Icon = "💊", Color = "#F44336", Type = CategoryType.Expense },
            new Category { Id = 10, Name = "Clothing",        Icon = "👕", Color = "#E91E63", Type = CategoryType.Expense },
            new Category { Id = 11, Name = "Other Expense",   Icon = "📦", Color = "#9E9E9E", Type = CategoryType.Expense }
        );
    }
}
