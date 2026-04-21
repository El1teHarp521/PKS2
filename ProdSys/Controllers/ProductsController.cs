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

        // Страница со списком продуктов
        public async Task<IActionResult> Index(string category, string searchString)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            ViewBag.Categories = await _context.Products
                .Where(p => p.Category != null)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            var products = await query.ToListAsync();

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

        // Страница управления составом (Материалами) продукта
        public async Task<IActionResult> Composition(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.AllMaterials = new SelectList(_context.Materials, "Id", "Name");
            return View(product);
        }

        // Добавить материал в состав
        [HttpPost]
        public async Task<IActionResult> AddMaterialToProduct(int productId, int materialId, decimal quantityNeeded)
        {
            var existing = await _context.ProductMaterials.FirstOrDefaultAsync(pm => pm.ProductId == productId && pm.MaterialId == materialId);
            
            if (existing != null) {
                existing.QuantityNeeded += quantityNeeded; 
            } else {
                _context.ProductMaterials.Add(new ProductMaterial {
                    ProductId = productId,
                    MaterialId = materialId,
                    QuantityNeeded = quantityNeeded
                });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Composition), new { id = productId });
        }
    }
}