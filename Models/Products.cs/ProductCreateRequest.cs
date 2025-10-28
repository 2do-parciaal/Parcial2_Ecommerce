namespace Parcial2_Ecommerce.Models.Products
{
    public class ProductCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CompanyId { get; set; }
    }
}