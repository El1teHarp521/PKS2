using System;

namespace ProdSys.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int? ProductionLineId { get; set; }
        
        public virtual ProductionLine? ProductionLine { get; set; }

        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Cancelled"
    }
}