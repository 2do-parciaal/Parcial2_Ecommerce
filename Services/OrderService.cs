using Microsoft.EntityFrameworkCore;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;

namespace Parcial2_Ecommerce.Services
{
    public class OrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db)
        {
            _db = db;
        }

        // Crea un pedido vacío (carrito) para un usuario si no tiene uno abierto
        public async Task<Order> GetOrCreateOpenOrderAsync(int userId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.UserId == userId && !o.IsPaid);

            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    IsPaid = false,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
            }

            return order;
        }

        // Agregar item al carrito
        public async Task<Order> AddItemAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var product = await _db.Products.FindAsync(productId)
                          ?? throw new InvalidOperationException("Producto no encontrado.");

            var order = await GetOrCreateOpenOrderAsync(userId);

            var existing = order.Items?.FirstOrDefault(i => i.ProductId == productId);
            if (existing == null)
            {
                existing = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Subtotal = product.Price * quantity
                };
                _db.OrderItems.Add(existing);
            }
            else
            {
                existing.Quantity += quantity;
                existing.Subtotal = product.Price * existing.Quantity;
            }

            await _db.SaveChangesAsync();

            // Recargar con navegación
            return await _db.Orders
                .Include(o => o.Items!)
                .ThenInclude(i => i.Product)
                .FirstAsync(o => o.Id == order.Id);
        }

        // Quitar item del carrito
        public async Task<Order> RemoveItemAsync(int userId, int orderItemId)
        {
            var order = await GetOrCreateOpenOrderAsync(userId);

            var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId && i.OrderId == order.Id)
                       ?? throw new InvalidOperationException("Item no encontrado en el carrito.");

            _db.OrderItems.Remove(item);
            await _db.SaveChangesAsync();

            return await _db.Orders
                .Include(o => o.Items!)
                .ThenInclude(i => i.Product)
                .FirstAsync(o => o.Id == order.Id);
        }

        // Checkout: valida stock y descuenta
        public async Task<Order> CheckoutAsync(int userId)
        {
            var order = await _db.Orders
                .Include(o => o.Items!)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.UserId == userId && !o.IsPaid)
                ?? throw new InvalidOperationException("No hay carrito abierto.");

            if (order.Items == null || order.Items.Count == 0)
                throw new InvalidOperationException("El carrito está vacío.");

            // Validar stock
            foreach (var item in order.Items)
            {
                if (item.Product == null)
                    throw new InvalidOperationException("Producto inválido en el carrito.");

                if (item.Quantity > item.Product.Stock)
                    throw new InvalidOperationException($"Stock insuficiente para {item.Product.Name}.");
            }

            // Descontar stock
            foreach (var item in order.Items)
            {
                item.Product!.Stock -= item.Quantity;
            }

            order.IsPaid = true;
            await _db.SaveChangesAsync();

            return order;
        }

        public Task<List<Order>> GetOrdersAsync(int? userId = null)
        {
            var query = _db.Orders
                .Include(o => o.Items!)
                .ThenInclude(i => i.Product)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            return query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }
    }
}
