using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Parcial2_Ecommerce.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Dueño de la empresa (usuario con rol Empresa)
        public int OwnerUserId { get; set; }

        [JsonIgnore]                      // evitar ciclos
        public User? OwnerUser { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}