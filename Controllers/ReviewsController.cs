using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Reviews;
using System.Security.Claims;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReviewsController(AppDbContext db) { _db = db; }

        private int CurrentUserId(ClaimsPrincipal user)
            => int.Parse(user.Claims.First(c => c.Type == "userId").Value);

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewCreateRequest req)
        {
            var uid = CurrentUserId(User);

            if (req.Rating < 1 || req.Rating > 5)
                return BadRequest("Rating debe ser 1 a 5.");

            var purchased = await _db.Orders
                .Include(o => o.Items!)
                .AnyAsync(o => o.UserId == uid && o.IsPaid &&
                               o.Items!.Any(i => i.ProductId == req.ProductId));
            if (!purchased)
                return Forbid("Solo clientes que compraron el producto pueden calificar.");

            var exists = await _db.Reviews.AnyAsync(r => r.UserId == uid && r.ProductId == req.ProductId);
            if (exists)
                return BadRequest("Ya existe una reseña tuya para este producto.");

            var review = new Review
            {
                UserId = uid,
                ProductId = req.ProductId,
                Rating = req.Rating,
                Comment = req.Comment
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok(review);
        }

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
