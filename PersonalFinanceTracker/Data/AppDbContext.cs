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
            new Category { Id = 1, Name = "Fizetés",       Icon = "💼", Color = "#4CAF50", Type = CategoryType.Income },
            new Category { Id = 2, Name = "Szabadúszó",    Icon = "💻", Color = "#8BC34A", Type = CategoryType.Income },
            new Category { Id = 3, Name = "Befektetés",    Icon = "📈", Color = "#009688", Type = CategoryType.Income },
            new Category { Id = 4, Name = "Egyéb bevétel", Icon = "💰", Color = "#00BCD4", Type = CategoryType.Income },
            new Category { Id = 5, Name = "Élelmiszer",    Icon = "🛒", Color = "#FF5722", Type = CategoryType.Expense },
            new Category { Id = 6, Name = "Rezsi",         Icon = "🏠", Color = "#795548", Type = CategoryType.Expense },
            new Category { Id = 7, Name = "Közlekedés",    Icon = "🚗", Color = "#607D8B", Type = CategoryType.Expense },
            new Category { Id = 8, Name = "Szórakozás",    Icon = "🎬", Color = "#9C27B0", Type = CategoryType.Expense },
            new Category { Id = 9, Name = "Egészség",      Icon = "💊", Color = "#F44336", Type = CategoryType.Expense },
            new Category { Id = 10, Name = "Ruházat",      Icon = "👕", Color = "#E91E63", Type = CategoryType.Expense },
            new Category { Id = 11, Name = "Egyéb kiadás", Icon = "📦", Color = "#9E9E9E", Type = CategoryType.Expense }
        );
    }
}
