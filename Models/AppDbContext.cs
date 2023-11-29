using App.Models.Contacts;
using App.Models.Posts;
using App.Models.Products;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            // Bỏ tiền tố AspNet của các bảng: mặc định các bảng trong IdentityDbContext có
            // tên với tiền tố AspNet như: AspNetUserRoles, AspNetUser ...
            // Đoạn mã sau chạy khi khởi tạo DbContext, tạo database sẽ loại bỏ tiền tố đó
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null && tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName[6..]);
                }
            }

            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.HasOne(p => p.Brand)
                        .WithMany(b => b.Products)
                        .HasForeignKey(p => p.BrandId)
                        .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                        .WithMany(u => u.Orders)
                        .HasForeignKey(o => o.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Brand>(entity =>
            {
                entity.HasIndex(b => b.Slug).IsUnique();
            });

            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });

            builder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(c => new
                {
                    c.CategoryId,
                    c.ProductId
                });
            });

            builder.Entity<Capacity>(entity =>
            {
                entity.Property(c => c.Sold).HasDefaultValue(0);
                entity.HasIndex(c => c.SellPrice);
            });

            builder.Entity<Review>(entity =>
            {
                entity.HasIndex(c => c.ProductId);
            });

            builder.Entity<Discount>(entity =>
            {
                entity.Property(d => d.MoneyDiscount).HasDefaultValue(0.0m);
                entity.Property(d => d.PercentDiscount).HasDefaultValue(0);
            });

            builder.Entity<ProductDiscount>(entity =>
            {
                entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductDiscounts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Discount)
                .WithMany(x => x.ProductDiscounts)
                .HasForeignKey(x => x.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasKey(x => new
                {
                    x.DiscountId,
                    x.ProductId
                });
            });

        }

        public DbSet<Product> Products { set; get; } = null!;
        public DbSet<Category> Categories { set; get; } = null!;
        public DbSet<ProductPhoto> ProductPhotos { set; get; } = null!;
        public DbSet<Brand> Brands { set; get; } = null!;
        public DbSet<ProductCategory> ProductCategories { set; get; } = null!;
        public DbSet<Color> Colors { set; get; } = null!;
        public DbSet<Capacity> Capacities { set; get; } = null!;
        public DbSet<Order> Orders { set; get; } = null!;
        public DbSet<OrderDetail> OrderDetails { set; get; } = null!;
        public DbSet<PriceLevel> PriceLevels { set; get; } = null!;
        public DbSet<PayStatus> PayStatuses { set; get; } = null!;
        public DbSet<Review> Reviews { set; get; } = null!;
        public DbSet<Post> Posts { set; get; } = null!;
        public DbSet<Contact> Contacts { set; get; } = null!;
        public DbSet<Discount> Discounts { set; get; } = null!;
        public DbSet<ProductDiscount> ProductDiscounts { set; get; } = null!;
    }
}