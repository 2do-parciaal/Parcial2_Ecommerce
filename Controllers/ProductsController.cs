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

        // Público: listar/ver
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Products.Include(p => p.Company).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _context.Products.Include(x => x.Company).FirstOrDefaultAsync(x => x.Id == id);
            return p is null ? NotFound() : Ok(p);
        }

        // Solo Empresa crea productos de su propia empresa
        [Authorize(Roles = "Empresa")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateRequest req)
        {
            var company = await _context.Companies.FindAsync(req.CompanyId);
            if (company == null) return BadRequest("La empresa (companyId) no existe.");

            var ownerId = CurrentUserId(User);
            if (company.OwnerUserId != ownerId)
                return Forbid("Solo puedes crear productos de tu propia empresa.");

            var p = new Product
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Stock = req.Stock,
                CompanyId = req.CompanyId
            };

            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
        }

        // Solo Empresa puede editar/eliminar sus productos
        [Authorize(Roles = "Empresa")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateRequest req)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            var company = await _context.Companies.FindAsync(req.CompanyId);
            if (company == null) return BadRequest("La empresa (companyId) no existe.");

            var ownerId = CurrentUserId(User);
            if (company.OwnerUserId != ownerId || p.CompanyId != company.Id)
                return Forbid("Solo puedes modificar productos de tu empresa.");

            p.Name = req.Name;
            p.Description = req.Description;
            p.Price = req.Price;
            p.Stock = req.Stock;
            p.CompanyId = req.CompanyId;

            await _context.SaveChangesAsync();
            return Ok(p);
        }

        [Authorize(Roles = "Empresa")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();

            var company = await _context.Companies.FindAsync(p.CompanyId);
            var ownerId = CurrentUserId(User);
            if (company == null || company.OwnerUserId != ownerId)
                return Forbid("Solo puedes eliminar productos de tu empresa.");

            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
