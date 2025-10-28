using System;
using System.Collections.Generic;

namespace Parcial2_Ecommerce.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;

        public ICollection<OrderItem>? Items { get; set; }
    }
}