using Microsoft.EntityFrameworkCore;
using LibraryProject.Models;

namespace LibraryProject.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlite("Data Source=library.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity => {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.ISBN).IsUnique();
                // Настройка связей многие-ко-многим
                entity.HasMany(b => b.Authors).WithMany(a => a.Books);
                entity.HasMany(b => b.Genres).WithMany(g => g.Books);
            });

            modelBuilder.Entity<Author>(entity => {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.FirstName, a.LastName }).IsUnique();
            });

            modelBuilder.Entity<Genre>(entity => {
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.Name).IsUnique();
            });
        }
    }
}