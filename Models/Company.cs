using System.Collections.Generic;

namespace Parcial2_Ecommerce.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Product>? Products { get; set; }
    }
}