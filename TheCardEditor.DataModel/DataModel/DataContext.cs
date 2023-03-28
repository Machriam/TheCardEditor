using Microsoft.EntityFrameworkCore;

namespace TheCardEditor.DataModel.DataModel;

public partial class DataContext : DbContext
{
    public virtual DbSet<Card> Cards { get; set; }
    public virtual DbSet<CardSet> CardSets { get; set; }
    public virtual DbSet<Font> Fonts { get; set; }
    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Picture> Pictures { get; set; }
    public virtual DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Card");

            entity.HasIndex(e => e.Name, "IX_Card_Name").IsUnique();

            entity.HasOne(d => d.CardSetFkNavigation).WithMany(p => p.Cards).HasForeignKey(d => d.CardSetFk);
        });

        modelBuilder.Entity<CardSet>(entity =>
        {
            entity.ToTable("CardSet");

            entity.HasOne(d => d.GameFkNavigation).WithMany(p => p.CardSets)
                .HasForeignKey(d => d.GameFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Font>(entity =>
        {
            entity.ToTable("Font");

            entity.Property(e => e.Name).HasDefaultValueSql("'Unnamed Font'");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Game");

            entity.HasIndex(e => e.Name, "IX_Game_Name").IsUnique();
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("Picture");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.ToTable("Template");

            entity.HasIndex(e => e.Name, "IX_Template_Name").IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
