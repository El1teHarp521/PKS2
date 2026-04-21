using Microsoft.EntityFrameworkCore;
using TouristGuide.Models;

namespace TouristGuide.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<City> Cities { get; set; }
        public DbSet<Attraction> Attractions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
                new City { 
                    Id = 1, Name = "Москва", Region = "Московская область", Population = 13000000,
                    History = "Москва — столица России, город федерального значения, крупнейший по численности населения город России.",
                    CoatOfArmsUrl = "/img/cities/moscow_coat.png",
                    PhotoUrl = "/img/cities/moscow.jpg"
                },
                new City {
                    Id = 2, Name = "Санкт-Петербург", Region = "Ленинградская область", Population = 5600000,
                    History = "Санкт-Петербург — второй по численности населения город России. Город федерального значения. Был основан 16 мая 1703 года Петром I.",
                    CoatOfArmsUrl = "/img/cities/spb_coat.png",
                    PhotoUrl = "/img/cities/spb.jpg"
                }
            );

            modelBuilder.Entity<Attraction>().HasData(
                new Attraction {
                    Id = 1, CityId = 1, Name = "Красная площадь", ShortDescription = "Главная и самая известная площадь Москвы.",
                    History = "Красная площадь расположена в центре Москвы, у северо-восточной стены Кремля. На площади расположены Лобное место, памятник Минину и Пожарскому, Мавзолей В. И. Ленина.",
                    WorkingHours = "Круглосуточно", TicketPrice = "Бесплатно",
                    PhotoUrl = "/img/attractions/red_square.jpg"
                },
                new Attraction {
                    Id = 2, CityId = 2, Name = "Эрмитаж", ShortDescription = "Государственный Эрмитаж — один из крупнейших и самых значимых художественных и культурно-исторических музеев мира.",
                    History = "Свою историю музей начинает с коллекций произведений искусства, которые приобретала в частном порядке российская императрица Екатерина II.",
                    WorkingHours = "11:00 – 18:00 (Вт, Чт, Вс), до 20:00 (Ср, Пт, Сб)", TicketPrice = "500 рублей",
                    PhotoUrl = "/img/attractions/hermitage.jpg"
                }
            );
        }
    }
}