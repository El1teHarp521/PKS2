using Microsoft.EntityFrameworkCore;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Data
{
    public class NetworkContext : DbContext
    {
        public DbSet<UrlHistory> Histories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Создаст файл network.db в папке с программой
            optionsBuilder.UseSqlite("Data Source=network.db");
        }
    }
}