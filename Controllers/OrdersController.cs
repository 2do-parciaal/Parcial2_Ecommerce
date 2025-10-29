using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Services;
using Parcial2_Ecommerce.Models.Orders;
using System.Security.Claims;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrdersController(OrderService orderService) => _orderService = orderService;

        private int CurrentUserId(ClaimsPrincipal user)
            => int.Parse(user.Claims.First(c => c.Type == "userId").Value);

        // === CARRITO (Cliente autenticado) ===

        [Authorize(Roles = "Cliente")]
        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            var uid = CurrentUserId(User);
            var order = await _orderService.GetOrCreateOpenOrderAsync(uid);
            return Ok(order);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddItem([FromBody] CartItemRequest request)
        {
            try
            {
                var uid = CurrentUserId(User);
                var updated = await _orderService.AddItemAsync(uid, request.ProductId, request.Quantity);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Cliente")]
        [HttpDelete("cart/remove/{orderItemId:int}")]
        public async Task<IActionResult> RemoveItem(int orderItemId)
        {
            try
            {
                var uid = CurrentUserId(User);
                var updated = await _orderService.RemoveItemAsync(uid, orderItemId);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("cart/checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var uid = CurrentUserId(User);
                var order = await _orderService.CheckoutAsync(uid);
                return Ok(new { message = "Compra completada con éxito.", order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // === PEDIDOS ===

        // Cliente: solo sus propios pedidos (implícito)
        [Authorize(Roles = "Cliente")]
        [HttpGet("my")]
        public async Task<IActionResult> MyOrders()
        {
            var uid = CurrentUserId(User);
            return Ok(await _orderService.GetOrdersAsync(uid));
        }

        // (Opcional) Admin: lista global (si no usas Admin, puedes borrar este endpoint)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _orderService.GetOrdersAsync(null));
    }
}
