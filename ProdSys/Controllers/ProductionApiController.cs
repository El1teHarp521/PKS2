using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProdSys.Data;
using ProdSys.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProdSys.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Страница со списком продукто
        public async Task<IActionResult> Index(string category, string searchString)
        {
            // 1. Сначала применяем фильтры для базы данных
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Выгружаем категории для выпадающего списка
            ViewBag.Categories = await _context.Products
                .Where(p => p.Category != null)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            // 2. Загружаем данные из БД в память
            var products = await query.ToListAsync();

            // 3. Выполняем поиск по тексту средствами C#
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products
                    .Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(products);
        }

        // Форма создания продукта
        public IActionResult Create()
        {
            ViewBag.Materials = new SelectList(_context.Materials, "Id", "Name");
            return View();
        }

        // Сохранение продукта
        [HttpPost]
        public async Task<IActionResult> Create(Product product, int[] selectedMaterials)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Привязываем выбранные материалы
                if (selectedMaterials != null && selectedMaterials.Length > 0)
                {
                    foreach (var materialId in selectedMaterials)
                    {
                        _context.ProductMaterials.Add(new ProductMaterial
                        {
                            ProductId = product.Id,
                            MaterialId = materialId,
                            QuantityNeeded = 1
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Materials = new SelectList(_context.Materials, "Id", "Name");
            return View(product);
        }
    }
}