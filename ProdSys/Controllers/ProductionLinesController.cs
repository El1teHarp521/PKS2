using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdSys.Data;
using System.Threading.Tasks;

namespace ProdSys.Controllers
{
    public class ProductionLinesController : Controller
    {
        private readonly AppDbContext _context;

        public ProductionLinesController(AppDbContext context)
        {
            _context = context;
        }

        // Панель всех линий
        public async Task<IActionResult> Index()
        {
            // Добавил восклицательный знак после CurrentWorkOrder, чтобы убрать warning
            var lines = await _context.ProductionLines
                .Include(l => l.CurrentWorkOrder!)
                .ThenInclude(o => o.Product)
                .ToListAsync();
            
            return View(lines);
        }

        // Завершить заказ и освободить линию
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int lineId)
        {
            var line = await _context.ProductionLines.Include(l => l.CurrentWorkOrder).FirstOrDefaultAsync(l => l.Id == lineId);
            if (line != null && line.CurrentWorkOrder != null)
            {
                line.CurrentWorkOrder.Status = "Completed"; // Завершаем заказ
                line.Status = "Stopped"; // Останавливаем линию
                line.CurrentWorkOrderId = null; // Отвязываем заказ
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Остановить линию аварийно
        [HttpPost]
        public async Task<IActionResult> StopLine(int lineId)
        {
            var line = await _context.ProductionLines.Include(l => l.CurrentWorkOrder).FirstOrDefaultAsync(l => l.Id == lineId);
            if (line != null && line.CurrentWorkOrder != null)
            {
                line.CurrentWorkOrder.Status = "Cancelled"; 
                line.Status = "Stopped";
                line.CurrentWorkOrderId = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Изменить коэффициент эффективности
        [HttpPost]
        public async Task<IActionResult> UpdateEfficiency(int lineId, float efficiency)
        {
            var line = await _context.ProductionLines.FindAsync(lineId);
            if (line != null && efficiency >= 0.5f && efficiency <= 2.0f)
            {
                line.EfficiencyFactor = efficiency;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}