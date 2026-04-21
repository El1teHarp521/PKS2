namespace ProdSys.Models
{
    public class ProductMaterial
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int MaterialId { get; set; }
        public virtual Material Material { get; set; } = null!;

        public decimal QuantityNeeded { get; set; }
    }
}