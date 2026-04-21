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

        public async Task<IActionResult> Index()
        {
            var orders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.ProductionLine)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, int quantity, int lineId)
        {
            var product = await _context.Products.Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material).FirstOrDefaultAsync(p => p.Id == productId);
            var line = await _context.ProductionLines.FindAsync(lineId);

            if (product == null || line == null || quantity <= 0) return RedirectToAction("Index");

            foreach (var pm in product.ProductMaterials)
            {
                if (pm.Material.Quantity < pm.QuantityNeeded * quantity)
                {
                    ModelState.AddModelError("", $"Недостаточно материала: {pm.Material.Name}");
                    await LoadViewBags();
                    return View();
                }
            }

            foreach (var pm in product.ProductMaterials) { pm.Material.Quantity -= (pm.QuantityNeeded * quantity); }

            int activeCount = _context.WorkOrders.Count(w => w.ProductionLineId == lineId && w.Status == "InProgress");
            double totalProdMinutes = quantity * product.ProductionTimePerUnit;
            double realMinutesNeeded = totalProdMinutes / (line.EfficiencyFactor > 0 ? line.EfficiencyFactor : 1.0);

            var order = new WorkOrder
            {
                ProductId = productId,
                ProductionLineId = lineId,
                Quantity = quantity,
                Status = activeCount < 7 ? "InProgress" : "Pending",
                StartDate = DateTime.Now,
                EstimatedEndDate = DateTime.Now.AddMinutes(realMinutesNeeded)
            };

            if (order.Status == "InProgress") line.Status = "Active";

            _context.WorkOrders.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "WorkOrders");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.WorkOrders.Include(o => o.ProductionLine).FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                order.Status = "Cancelled";
                var line = order.ProductionLine;
                if (line != null)
                {
                    var next = _context.WorkOrders.Where(w => w.ProductionLineId == line.Id && w.Status == "Pending").OrderBy(w => w.Id).FirstOrDefault();
                    if (next != null)
                    {
                        var p = _context.Products.Find(next.ProductId);
                        next.Status = "InProgress";
                        next.StartDate = DateTime.Now;
                        double totalWork = next.Quantity * (p?.ProductionTimePerUnit ?? 1);
                        next.EstimatedEndDate = DateTime.Now.AddMinutes(totalWork / line.EfficiencyFactor);
                    }
                    if (!_context.WorkOrders.Any(w => w.ProductionLineId == line.Id && w.Status == "InProgress")) line.Status = "Stopped";
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        private async Task LoadViewBags()
        {
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            ViewBag.Lines = new SelectList(await _context.ProductionLines.ToListAsync(), "Id", "Name");
        }
    }
}