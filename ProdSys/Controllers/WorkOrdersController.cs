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
    public class WorkOrdersController : Controller
    {
        private readonly AppDbContext _context;

        public WorkOrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Список всех заказов
        public async Task<IActionResult> Index()
        {
            var orders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.ProductionLine)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: Форма создания заказа
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View();
        }

        // POST: Запустить в производство
        [HttpPost]
        public async Task<IActionResult> Create(int productId, int quantity, int lineId)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Количество должно быть больше нуля!");
                await LoadViewBags();
                return View();
            }

            // Получаем продукт вместе с привязанными материалами
            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .ThenInclude(pm => pm.Material)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var line = await _context.ProductionLines.FindAsync(lineId);
            if (line == null) return NotFound();

            // 1. Проверка: хватает ли материалов на складе
            foreach (var pm in product.ProductMaterials)
            {
                var totalNeeded = pm.QuantityNeeded * quantity;
                if (pm.Material.Quantity < totalNeeded)
                {
                    ModelState.AddModelError("", $"На складе не хватает: {pm.Material.Name}. Нужно: {totalNeeded}, В наличии: {pm.Material.Quantity}");
                    await LoadViewBags();
                    return View();
                }
            }

            // 2. Если всё хватает -> списываем материалы со склада
            foreach (var pm in product.ProductMaterials)
            {
                pm.Material.Quantity -= (pm.QuantityNeeded * quantity);
            }

            // 3. Авторасчет времени (Формула из задания)
            double efficiency = line.EfficiencyFactor > 0 ? line.EfficiencyFactor : 1.0;
            double totalMinutes = (quantity * product.ProductionTimePerUnit) / efficiency;

            // 4. Создаем заказ
            var order = new WorkOrder
            {
                ProductId = productId,
                ProductionLineId = lineId,
                Quantity = quantity,
                StartDate = DateTime.Now,
                EstimatedEndDate = DateTime.Now.AddMinutes(totalMinutes),
                Status = "InProgress" // Ставим статус "В процессе"
            };

            // 5. Обновляем статус линии
            line.Status = "Active";

            _context.WorkOrders.Add(order);
            await _context.SaveChangesAsync();

            // Привязываем заказ к линии после сохранения (нужен ID заказа)
            line.CurrentWorkOrderId = order.Id;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Отменить заказ
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.WorkOrders.FindAsync(id);
            if (order != null && order.Status == "InProgress")
            {
                order.Status = "Cancelled";
                
                // Освобождаем производственную линию
                var line = await _context.ProductionLines.FirstOrDefaultAsync(l => l.Id == order.ProductionLineId);
                if (line != null)
                {
                    line.Status = "Stopped";
                    line.CurrentWorkOrderId = null;
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для загрузки выпадающих списков
        private async Task LoadViewBags()
        {
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            // Показываем только свободные линии (Status == Stopped)
            var availableLines = await _context.ProductionLines.Where(l => l.Status == "Stopped").ToListAsync();
            ViewBag.Lines = new SelectList(availableLines, "Id", "Name");
        }
    }
}