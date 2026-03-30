using Blace.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blace.Server.Data;

public class Db(DbContextOptions<Db> options) : DbContext(options)
{
    public DbSet<PlaceEntity> Places { get; set; } = null!;
    public DbSet<TileEntity> Tiles { get; set; } = null!;
    public DbSet<DeleteEntity> Deletes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Place entity
        modelBuilder.Entity<PlaceEntity>(entity =>
        {
            entity.ToTable("places");
            entity.Property(p => p.Id).HasMaxLength(450);
            entity.Property(p => p.Title).HasMaxLength(200);
            entity.Property(p => p.Canvas).HasColumnType("bytea");
        });

        // Configure Tile entity
        modelBuilder.Entity<TileEntity>(entity =>
        {
            entity.ToTable("tiles");
            entity.Property(t => t.Id).HasMaxLength(450);
            entity.Property(t => t.PlaceId).HasMaxLength(450);
            entity.Property(t => t.DeleteId).HasMaxLength(450);
            
            entity.HasIndex(t => t.PlaceId);
            entity.HasIndex(t => new { t.PlaceId, t.X, t.Y });
            entity.HasIndex(t => new { t.PlaceId, t.UserId });
            entity.HasIndex(t => t.DeleteId);
        });

        // Configure Delete entity
        modelBuilder.Entity<DeleteEntity>(entity =>
        {
            entity.ToTable("deletes");
            entity.Property(d => d.Id).HasMaxLength(450);
        });
    }
}