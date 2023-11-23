using System.Collections;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TheCardEditor.DataModel.Migrations;

namespace TheCardEditor.DataModel.DataModel;

public partial class DataContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public void Migrate()
    {
        var version = Database.SqlQueryRaw<string>("select value from Application where Name='Version'");
        var resourceSet = DatabaseMigrations.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, false) ??
            throw new Exception("No Migration Resource found");
        var currentVersion = "0.0.0";
        if (version != null) currentVersion = version.FirstOrDefault() ?? currentVersion;
        foreach (var sqlToApply in IVersionSort.CreateDefault(currentVersion).GetPatchesToApply(resourceSet))
        {
            Database.ExecuteSqlRaw(sqlToApply.SQL);
        }
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
