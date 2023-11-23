using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheCardEditor.DataModel.DataModel;

#if DEBUG

public class DataContextDesignFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlite(@"Data Source=C:\Users\GruselGusel\Desktop\Repos\TheCardEditor\data.sqlite3");

        return new DataContext(optionsBuilder.Options);
    }
}

#endif

public partial class DataContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
