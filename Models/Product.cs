using System.Text.Json.Serialization;

namespace Parcial2_Ecommerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public int CompanyId { get; set; }

        // Evita ciclo Company -> Products -> Product -> Company -> ...
        [JsonIgnore]
        public Company? Company { get; set; }
    }
}