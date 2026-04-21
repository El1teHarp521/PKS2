using Microsoft.EntityFrameworkCore;
using ProdSys.Models;

namespace ProdSys.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.ProductionLine)
                .WithMany(l => l.WorkOrders)
                .HasForeignKey(w => w.ProductionLineId);

            modelBuilder.Entity<ProductionLine>().HasData(
                new ProductionLine { Id = 1, Name = "Линия А (Сборка)", Status = "Stopped", EfficiencyFactor = 1.0f },
                new ProductionLine { Id = 2, Name = "Линия Б (Упаковка)", Status = "Stopped", EfficiencyFactor = 1.0f }
            );

            modelBuilder.Entity<Material>().HasData(
                new Material { Id = 1, Name = "Сталь", Quantity = 1000, UnitOfMeasure = "кг", MinimalStock = 200 },
                new Material { Id = 2, Name = "Пластик", Quantity = 50, UnitOfMeasure = "кг", MinimalStock = 100 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Корпус системного блока", Category = "Комплектующие", Quantity = 5, MinimalStock = 10, ProductionTimePerUnit = 30 }
            );

            modelBuilder.Entity<ProductMaterial>().HasData(
                new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 2 },
                new ProductMaterial { ProductId = 1, MaterialId = 2, QuantityNeeded = 1 }
            );
        }
    }
}