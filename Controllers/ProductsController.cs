using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Products;
using System.Security.Claims;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductsController(AppDbContext context) => _context = context;

        int CurrentUserId(ClaimsPrincipal user)
            => int.Parse(user.Claims.First(c => c.Type == "userId").Value);

        // Público
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Products.Include(p => p.Company).ToListAsync());

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _context.Products.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
            return p is null ? NotFound() : Ok(p);
        }

        // Empresa: crea en su propia empresa (implícita)
        [Authorize(Roles = "Empresa")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateRequest req)
        {
            var ownerId = CurrentUserId(User);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null)
                return BadRequest("No tienes una empresa registrada. Crea tu empresa antes de publicar productos.");

            var p = new Product
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Stock = req.Stock,
                CompanyId = company.Id
            };

            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
        }

        // Empresa: modifica solo productos de su empresa (implícito)
        [Authorize(Roles = "Empresa")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateRequest req)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            var ownerId = CurrentUserId(User);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null || p.CompanyId != company.Id)
                return Forbid("Solo puedes modificar productos de tu empresa.");

            p.Name = req.Name;
            p.Description = req.Description;
            p.Price = req.Price;
            p.Stock = req.Stock;

            await _context.SaveChangesAsync();
            return Ok(p);
        }

        // Empresa: elimina solo productos de su empresa (implícito)
        [Authorize(Roles = "Empresa")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            var ownerId = CurrentUserId(User);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null || p.CompanyId != company.Id)
                return Forbid("Solo puedes eliminar productos de tu empresa.");

            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
