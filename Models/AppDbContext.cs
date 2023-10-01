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

        protected override void OnModelCreating (ModelBuilder builder) {

            base.OnModelCreating (builder); 
            // Bỏ tiền tố AspNet của các bảng: mặc định các bảng trong IdentityDbContext có
            // tên với tiền tố AspNet như: AspNetUserRoles, AspNetUser ...
            // Đoạn mã sau chạy khi khởi tạo DbContext, tạo database sẽ loại bỏ tiền tố đó
            foreach (var entityType in builder.Model.GetEntityTypes ()) {
                var tableName = entityType.GetTableName ();
                if (tableName != null && tableName.StartsWith("AspNet")) {
                    entityType.SetTableName(tableName[6..]);
                }
            }

            builder.Entity<Product>(entity => {
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.Property(p => p.Sold).HasDefaultValue(0);
            });

            builder.Entity<Brand>(entity => {
                entity.HasIndex(b => b.Slug).IsUnique();
            });

            builder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug).IsUnique();
            });

            builder.Entity<ProductCategory>(entity => {
                entity.HasKey(c => new {
                    c.CategoryId,
                    c.ProductId
                });
            });

            

        }

        public required DbSet<Product> Products {set;get;}
        public required DbSet<Category> Categories {set;get;}

        public required DbSet<ProductPhoto> ProductPhotos {set;get;}
        public required DbSet<Brand> Brands {set;get;}
        public required DbSet<ProductCategory> ProductCategories {set;get;}

        public required DbSet<Color> Colors {set;get;}
        public required DbSet<Capacity> Capacities {set;get;}
    }
}