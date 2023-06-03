using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using ShortcutManager.Model;

namespace ShortcutManager.Config;

public class MyDbContext: DbContext
{
    public DbSet<Data> Datas { get; set; }
    public DbSet<Verb> Verbs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DataNCategory> DataNCategories { get; set; }
    public DbSet<DataNVerb> DataNVerbs { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(Environment.CurrentDirectory, @"data.db");
        optionsBuilder.UseSqlite(
            $"Data Source={dbPath}");
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Data>().HasIndex(d => d.RealPath).IsUnique();
    }
}