namespace Lab11_12;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=notes.db");
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }
    
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<User> Users => Set<User>();
}