using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using xShapes.Models;

namespace xShapes.Services
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Inventory> Inventory { get; set; } = null!;
        public virtual DbSet<InventoryItems> InventoryItems { get; set; } = null!;
        public virtual DbSet<ItemsTransaction> ItemsTransaction { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
        public virtual DbSet<Shape> Shape { get; set; } = null!;
        public virtual DbSet<Users> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable(tb => tb.IsTemporal(ttb =>
    {
        ttb.UseHistoryTable("Inventory_History", "dbo");
        ttb
            .HasPeriodStart("ValidFrom")
            .HasColumnName("ValidFrom");
        ttb
            .HasPeriodEnd("ValidTo")
            .HasColumnName("ValidTo");
    }
));

                entity.Property(e => e.InventoryId).HasColumnName("InventoryID");

                entity.Property(e => e.Location).HasMaxLength(30);

                entity.HasMany(d => d.User)
                    .WithMany(p => p.Inventory)
                    .UsingEntity<Dictionary<string, object>>(
                        "AssignedInventory",
                        l => l.HasOne<Users>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__AssignedI__UserI__29221CFB"),
                        r => r.HasOne<Inventory>().WithMany().HasForeignKey("InventoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__AssignedI__Inven__282DF8C2"),
                        j =>
                        {
                            j.HasKey("InventoryId", "UserId");

                            j.ToTable("AssignedInventory");

                            j.IndexerProperty<int>("InventoryId").HasColumnName("InventoryID");

                            j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                        });
            });

            modelBuilder.Entity<InventoryItems>(entity =>
            {
                entity.HasKey(e => e.ItemId)
                    .HasName("PK__Inventor__727E83EB3CDA6B40");

                entity.ToTable(tb => tb.IsTemporal(ttb =>
    {
        ttb.UseHistoryTable("InventoryItems_History", "dbo");
        ttb
            .HasPeriodStart("ValidFrom")
            .HasColumnName("ValidFrom");
        ttb
            .HasPeriodEnd("ValidTo")
            .HasColumnName("ValidTo");
    }
));

                entity.Property(e => e.ItemId).HasColumnName("ItemID");

                entity.Property(e => e.InventoryId).HasColumnName("InventoryID");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ShapeId).HasColumnName("ShapeID");

                entity.HasOne(d => d.Inventory)
                    .WithMany(p => p.InventoryItems)
                    .HasForeignKey(d => d.InventoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Inventory__Inven__245D67DE");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InventoryItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Inventory__Produ__236943A5");

                entity.HasOne(d => d.Shape)
                    .WithMany(p => p.InventoryItems)
                    .HasForeignKey(d => d.ShapeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Inventory__Shape__22751F6C");
            });

            modelBuilder.Entity<ItemsTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PK__ItemsTra__55433A4B15FD043F");

                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

                entity.Property(e => e.InventoryId).HasColumnName("InventoryID");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.HasOne(d => d.Inventory)
                    .WithMany(p => p.ItemsTransaction)
                    .HasForeignKey(d => d.InventoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ItemsTran__Inven__2EDAF651");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ItemsTransaction)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ItemsTran__Produ__2DE6D218");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable(tb => tb.IsTemporal(ttb =>
    {
        ttb.UseHistoryTable("Product_History", "dbo");
        ttb
            .HasPeriodStart("ValidFrom")
            .HasColumnName("ValidFrom");
        ttb
            .HasPeriodEnd("ValidTo")
            .HasColumnName("ValidTo");
    }
));

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.Name).HasMaxLength(30);
            });

            modelBuilder.Entity<Shape>(entity =>
            {
                entity.ToTable(tb => tb.IsTemporal(ttb =>
    {
        ttb.UseHistoryTable("Shape_History", "dbo");
        ttb
            .HasPeriodStart("ValidFrom")
            .HasColumnName("ValidFrom");
        ttb
            .HasPeriodEnd("ValidTo")
            .HasColumnName("ValidTo");
    }
));

                entity.Property(e => e.ShapeId).HasColumnName("ShapeID");

                entity.Property(e => e.Name).HasMaxLength(30);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__Users__1788CCACE757FDD2");

                entity.ToTable(tb => tb.IsTemporal(ttb =>
    {
        ttb.UseHistoryTable("Users_History", "dbo");
        ttb
            .HasPeriodStart("ValidFrom")
            .HasColumnName("ValidFrom");
        ttb
            .HasPeriodEnd("ValidTo")
            .HasColumnName("ValidTo");
    }
));

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.ApiKey).HasMaxLength(200);

                entity.Property(e => e.ContactNo).HasMaxLength(30);

                entity.Property(e => e.Email).HasMaxLength(30);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(30);

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.Role).HasMaxLength(30);

                entity.Property(e => e.Username).HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
