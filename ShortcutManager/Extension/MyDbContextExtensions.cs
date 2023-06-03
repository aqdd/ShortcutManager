using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace ShortcutManager.Extension;

internal static class MyDbContextExtensions
{
    internal static void EnsureCreatingMissingTables<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext
    {
        var type = typeof(TDbContext);
        var dbSetType = typeof(DbSet<>);

        var dbPropertyNames = type.GetProperties().Where(p => p.PropertyType.Name == dbSetType.Name)
            .Select(p => p.Name).ToArray();

        foreach (var entityName in dbPropertyNames)
        {
            CheckTableExistsAndCreateIfMissing(dbContext, entityName);
        }
    }

    private static void CheckTableExistsAndCreateIfMissing(DbContext dbContext, string entityName)
    {
        var defaultSchema = dbContext.Model.GetDefaultSchema();
        var tableName = string.IsNullOrWhiteSpace(defaultSchema)
            ? $"{entityName}"
            : $"{defaultSchema}.{entityName}";

        try
        {
            _ = dbContext.Database.ExecuteSqlRaw($"SELECT * FROM {tableName} limit 1"); //Throws on missing table
        }
        catch (Exception)
        {
            var scriptStart = $"CREATE TABLE \"{tableName}\"";
            // const string scriptEnd = "GO";
            var script = dbContext.Database.GenerateCreateScript();

            var reg = new Regex($"(?<table>{scriptStart} [\\(][^);]+[\\)];)");
            var m = reg.Match(script);
            if (m.Success)
            {
                var t = m.Groups["table"].Value;
                dbContext.Database.ExecuteSqlRaw(t);
            }
            // var tableScript = script.Split(scriptStart).Last().Split(";");
            // var first = $"{scriptStart} {tableScript.First()}";
            // dbContext.Database.ExecuteSqlRaw(first);
        }
    }
}