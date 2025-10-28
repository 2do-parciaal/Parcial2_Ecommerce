using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Services;
using Parcial2_Ecommerce.Models.Orders;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrdersController(OrderService orderService) => _orderService = orderService;

        // Solo Cliente usa carrito
        [Authorize(Roles = "Cliente")]
        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCart(int userId)
            => Ok(await _orderService.GetOrCreateOpenOrderAsync(userId));

        [Authorize(Roles = "Cliente")]
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddItem([FromBody] CartItemRequest request)
        {
            try { return Ok(await _orderService.AddItemAsync(request.UserId, request.ProductId, request.Quantity)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [Authorize(Roles = "Cliente")]
        [HttpDelete("cart/remove/{userId}/{orderItemId}")]
        public async Task<IActionResult> RemoveItem(int userId, int orderItemId)
        {
            try { return Ok(await _orderService.RemoveItemAsync(userId, orderItemId)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("cart/checkout/{userId}")]
        public async Task<IActionResult> Checkout(int userId)
        {
            try { return Ok(new { message = "Compra completada con éxito.", order = await _orderService.CheckoutAsync(userId) }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // Listar pedidos (público/simple). Puedes poner reglas si lo necesitas.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId = null)
            => Ok(await _orderService.GetOrdersAsync(userId));
    }
}
