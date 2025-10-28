using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context) => _context = context;

        [HttpPost("create")]
        public IActionResult Create([FromBody] Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
            return Ok(order);
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Orders.ToList());
    }
}