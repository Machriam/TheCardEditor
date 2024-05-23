using Microsoft.EntityFrameworkCore;

namespace TheCardEditor.DataModel.DataModel;

public partial class DataContext : DbContext
{
    public virtual DbSet<ApplicationDatum> ApplicationData { get; set; }
    public virtual DbSet<Card> Cards { get; set; }
    public virtual DbSet<CardSet> CardSets { get; set; }
    public virtual DbSet<Font> Fonts { get; set; }
    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Picture> Pictures { get; set; }
    public virtual DbSet<PictureCardReference> PictureCardReferences { get; set; }
    public virtual DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Card");

            entity.HasIndex(e => e.CardSetFk, "IX_Card_CardSetFk");

            entity.HasOne(d => d.CardSetFkNavigation).WithMany(p => p.Cards)
                .HasForeignKey(d => d.CardSetFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CardSet>(entity =>
        {
            entity.ToTable("CardSet");

            entity.HasIndex(e => e.GameFk, "IX_CardSet_GameFk");

            entity.Property(e => e.Zoom).HasDefaultValue(100.0);

            entity.HasOne(d => d.GameFkNavigation).WithMany(p => p.CardSets)
                .HasForeignKey(d => d.GameFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Font>(entity =>
        {
            entity.ToTable("Font");

            entity.Property(e => e.Name).HasDefaultValue("Unnamed Font");
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

        modelBuilder.Entity<PictureCardReference>(entity =>
        {
            entity.ToTable("PictureCardReference");

            entity.HasOne(d => d.CardFkNavigation).WithMany(p => p.PictureCardReferences)
                .HasForeignKey(d => d.CardFk)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PictureFkNavigation).WithMany(p => p.PictureCardReferences)
                .HasForeignKey(d => d.PictureFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.ToTable("Template");

            entity.HasIndex(e => e.CardSetFk, "IX_Template_CardSetFk");

            entity.HasIndex(e => e.Name, "IX_Template_Name").IsUnique();

            entity.HasOne(d => d.CardSetFkNavigation).WithMany(p => p.Templates)
                .HasForeignKey(d => d.CardSetFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
