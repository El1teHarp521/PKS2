using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TouristGuide.Data;

namespace TouristGuide.Controllers
{
    public class AttractionsController : Controller
    {
        private readonly AppDbContext _context;

        public AttractionsController(AppDbContext context)
        {
            _context = context;
        }

        // Страница конкретной достопримечательности
        public IActionResult Details(int id)
        {
            var attraction = _context.Attractions
                .Include(a => a.City)
                .FirstOrDefault(a => a.Id == id);

            if (attraction == null) return NotFound();

            return View(attraction);
        }
    }
}