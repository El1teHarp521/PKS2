using System.Collections.Generic;

namespace ProdSys.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Specifications { get; set; }
        public string? Category { get; set; }
        
        public int Quantity { get; set; }
        public int MinimalStock { get; set; }
        public int ProductionTimePerUnit { get; set; }

        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}