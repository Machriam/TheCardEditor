using Microsoft.EntityFrameworkCore;

namespace TheCardEditor.DataModel.DataModel;

public partial class DataContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
