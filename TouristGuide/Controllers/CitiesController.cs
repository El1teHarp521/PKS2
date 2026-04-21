using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TouristGuide.Data;

namespace TouristGuide.Controllers
{
    public class CitiesController : Controller
    {
        private readonly AppDbContext _context;

        public CitiesController(AppDbContext context)
        {
            _context = context;
        }

        // Главная страница: список городов + поиск
        public IActionResult Index(string searchString)
        {
            var cities = _context.Cities.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                cities = cities.Where(c => c.Name.Contains(searchString));
            }

            return View(cities.ToList());
        }

        // Страница конкретного города
        public IActionResult Details(int id)
        {
            var city = _context.Cities
                .Include(c => c.Attractions)
                .FirstOrDefault(c => c.Id == id);

            if (city == null) return NotFound();

            return View(city);
        }
    }
}