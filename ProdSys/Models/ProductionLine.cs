using System.Collections.Generic;

namespace ProdSys.Models
{
    public class ProductionLine
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Stopped"; 
        public float EfficiencyFactor { get; set; } = 1.0f;
        
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>(); 
    }
}