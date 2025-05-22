using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace restaurant_app.Models;

public partial class RestaurantDbContext : DbContext
{
    public RestaurantDbContext()
    {
    }

    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Allergen> Allergens { get; set; }

    public virtual DbSet<AppConfig> AppConfigs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuProduct> MenuProducts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-145H9U0\\SQLEXPRESS;Database=RestaurantDB;Trusted_Connection=True;Encrypt=false;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.AllergenId).HasName("PK__Allergen__158B937F5D905C62");

            entity.Property(e => e.AllergenId).HasColumnName("AllergenID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<AppConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigKey).HasName("PK__AppConfi__4A30678502367D1D");

            entity.ToTable("AppConfig");

            entity.Property(e => e.ConfigKey).HasMaxLength(50);
            entity.Property(e => e.ConfigValue).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B7DA44846");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menus__C99ED250E8C239EC");

            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Menus)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menus_Categories");
        });

        modelBuilder.Entity<MenuProduct>(entity =>
        {
            entity.HasKey(e => new { e.MenuId, e.ProductId }).HasName("PK__MenuProd__02DE1E3EBC90F0AF");

            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ProductQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductUnit).HasMaxLength(10);

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuProducts)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK_MenuProducts_Menus");

            entity.HasOne(d => d.Product).WithMany(p => p.MenuProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuProducts_Products");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF628B20A5");

            entity.HasIndex(e => e.OrderCode, "UQ__Orders__999B5229975749A2").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(255);
            entity.Property(e => e.DeliveryFee)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstimatedDeliveryTime).HasColumnType("datetime");
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderCode).HasMaxLength(20);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Registered");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C1CC61BD3");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.MenuId).HasColumnName("MenuID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Menu).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK_OrderDetails_Menus");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED555D943A");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PortionQuantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PortionUnit).HasMaxLength(10);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalQuantity).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasMany(d => d.Allergens).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductAllergen",
                    r => r.HasOne<Allergen>().WithMany()
                        .HasForeignKey("AllergenId")
                        .HasConstraintName("FK_ProductAllergens_Allergens"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK_ProductAllergens_Products"),
                    j =>
                    {
                        j.HasKey("ProductId", "AllergenId").HasName("PK__ProductA__55547FDA2BCDC157");
                        j.ToTable("ProductAllergens");
                        j.IndexerProperty<int>("ProductId").HasColumnName("ProductID");
                        j.IndexerProperty<int>("AllergenId").HasColumnName("AllergenID");
                    });
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F4EC7E6ACC08");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImages_Products");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC1D082027");

            // Keep the index but make it optional - change to a non-unique index if needed
            // or keep it but allow null values
            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534118A053A");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired(); // Make Username required
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .HasDefaultValue("Client");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
