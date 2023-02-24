using Microsoft.EntityFrameworkCore;

namespace TheCardEditor.DataModel.DataModel;

public partial class DataContext : DbContext
{
    public virtual DbSet<Card> Cards { get; set; }
    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Layer> Layers { get; set; }
    public virtual DbSet<Picture> Pictures { get; set; }
    public virtual DbSet<Template> Templates { get; set; }
    public virtual DbSet<TemplateLayerAssociation> TemplateLayerAssociations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Card");

            entity.HasIndex(e => e.Name, "IX_Card_Name").IsUnique();

            entity.HasOne(d => d.GameFkNavigation).WithMany(p => p.Cards)
                .HasForeignKey(d => d.GameFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Game");

            entity.HasIndex(e => e.Name, "IX_Game_Name").IsUnique();
        });

        modelBuilder.Entity<Layer>(entity =>
        {
            entity.ToTable("Layer");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.CardFkNavigation).WithMany(p => p.Layers)
                .HasForeignKey(d => d.CardFk)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PictureFkNavigation).WithMany(p => p.Layers)
                .HasForeignKey(d => d.PictureFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
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

        modelBuilder.Entity<TemplateLayerAssociation>(entity =>
        {
            entity.ToTable("TemplateLayerAssociation");

            entity.HasOne(d => d.LayerFkNavigation).WithMany(p => p.TemplateLayerAssociations)
                .HasForeignKey(d => d.LayerFk)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.TemplateFkNavigation).WithMany(p => p.TemplateLayerAssociations)
                .HasForeignKey(d => d.TemplateFk)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
