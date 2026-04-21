using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdSys.Data;
using ProdSys.Models;

namespace ProdSys.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Отображение таблицы материалов
        public async Task<IActionResult> Index()
        {
            var materials = await _context.Materials.ToListAsync();
            return View(materials);
        }

        // GET: Страница создания нового материала
        public IActionResult Create()
        {
            return View();
        }

        // POST: Сохранение нового материала в БД
        [HttpPost]
        public async Task<IActionResult> Create(Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(material);
        }

        // POST: Быстрое пополнение склада
        [HttpPost]
        public async Task<IActionResult> Replenish(int id, decimal amount)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null && amount > 0)
            {
                material.Quantity += amount;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}