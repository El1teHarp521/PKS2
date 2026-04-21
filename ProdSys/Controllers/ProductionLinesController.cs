using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProdSys.Data;
using System;
using System.Linq;
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

        public async Task<IActionResult> Index()
        {
            var lines = await _context.ProductionLines
                .Include(l => l.WorkOrders)
                .ThenInclude(o => o.Product)
                .ToListAsync();
            return View(lines);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId, int lineId)
        {
            var order = await _context.WorkOrders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null) 
            {
                order.Status = "Completed";
                order.Product.Quantity += order.Quantity;
            }

            var line = await _context.ProductionLines.Include(l => l.WorkOrders).ThenInclude(w => w.Product).FirstOrDefaultAsync(l => l.Id == lineId);
            
            var activeCount = line!.WorkOrders.Count(w => w.Status == "InProgress");
            if (activeCount < 7)
            {
                var next = line.WorkOrders.Where(w => w.Status == "Pending").OrderBy(w => w.Id).FirstOrDefault();
                if (next != null)
                {
                    next.Status = "InProgress";
                    next.StartDate = DateTime.Now;
                    double mins = (next.Quantity * next.Product.ProductionTimePerUnit) / line.EfficiencyFactor;
                    next.EstimatedEndDate = DateTime.Now.AddMinutes(mins);
                }
            }

            if (!line.WorkOrders.Any(w => w.Status == "InProgress")) line.Status = "Stopped";

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEfficiency(int lineId, float efficiency)
        {
            var line = await _context.ProductionLines.Include(l => l.WorkOrders).ThenInclude(w => w.Product).FirstOrDefaultAsync(l => l.Id == lineId);
            if (line != null && efficiency >= 0.5f && efficiency <= 100.0f)
            {
                float oldEff = line.EfficiencyFactor;
                line.EfficiencyFactor = efficiency;
                DateTime now = DateTime.Now;

                foreach (var order in line.WorkOrders.Where(w => w.Status == "InProgress"))
                {
                    double minutesPassed = (now - order.StartDate).TotalMinutes;
                    double workDone = minutesPassed * oldEff;
                    double totalWorkNeeded = order.Quantity * order.Product.ProductionTimePerUnit;
                    double workRemaining = totalWorkNeeded - workDone;

                    if (workRemaining < 0) workRemaining = 0;

                    order.StartDate = now;
                    double newRealMinutesLeft = workRemaining / efficiency;
                    order.EstimatedEndDate = now.AddMinutes(newRealMinutesLeft);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> StopLine(int lineId)
        {
            var line = await _context.ProductionLines.Include(l => l.WorkOrders).FirstOrDefaultAsync(l => l.Id == lineId);
            if (line != null)
            {
                foreach (var order in line.WorkOrders.Where(w => w.Status == "InProgress"))
                {
                    order.Status = "Cancelled";
                }
                line.Status = "Stopped";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}