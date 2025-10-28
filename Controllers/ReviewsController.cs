using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Reviews;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReviewsController(AppDbContext db) { _db = db; }

        // Solo clientes pueden opinar Y solo si compraron el producto
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewCreateRequest req)
        {
            if (req.Rating < 1 || req.Rating > 5)
                return BadRequest("Rating debe ser 1 a 5.");

            // Verificar compra (pedido pagado con ese producto)
            var purchased = await _db.Orders
                .Include(o => o.Items!)
                .AnyAsync(o => o.UserId == req.UserId &&
                               o.IsPaid &&
                               o.Items!.Any(i => i.ProductId == req.ProductId));
            if (!purchased)
                return Forbid("Solo clientes que compraron el producto pueden calificar.");

            // Impedir doble review del mismo usuario para el mismo producto
            var exists = await _db.Reviews
                .AnyAsync(r => r.UserId == req.UserId && r.ProductId == req.ProductId);
            if (exists)
                return BadRequest("Ya existe una reseña tuya para este producto.");

            var review = new Review
            {
                UserId = req.UserId,
                ProductId = req.ProductId,
                Rating = req.Rating,
                Comment = req.Comment
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok(review);
        }

        // Público: ver reseñas de un producto
        [AllowAnonymous]
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> ByProduct(int productId)
        {
            var list = await _db.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return Ok(list);
        }
    }
}
