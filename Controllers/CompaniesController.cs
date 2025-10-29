using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Companies;
using System.Security.Claims;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CompaniesController(AppDbContext context) => _context = context;

        int CurrentUserId(ClaimsPrincipal user)
            => int.Parse(user.Claims.First(c => c.Type == "userId").Value);

        // === Públicos para exploración ===
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies
                .Include(c => c.Products)
                .ToListAsync();
            return Ok(companies);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return NotFound();
            return Ok(company);
        }

        // === Implícitos para dueños (rol Empresa) ===

        // Info de mi propia empresa
        [Authorize(Roles = "Empresa")]
        [HttpGet("me")]
        public async Task<IActionResult> MyCompany()
        {
            var ownerId = CurrentUserId(User);
            var company = await _context.Companies
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null) return NotFound("No tienes una empresa registrada.");
            return Ok(company);
        }

        // Crear mi empresa (si no tengo una)
        [Authorize(Roles = "Empresa")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest("El nombre es obligatorio.");

            var ownerId = CurrentUserId(User);
            var already = await _context.Companies.AnyAsync(c => c.OwnerUserId == ownerId);
            if (already) return BadRequest("Ya tienes una empresa registrada con este usuario.");

            var company = new Company { Name = req.Name, OwnerUserId = ownerId };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(MyCompany), new { }, company);
        }

        // Actualizar mi empresa (implícita)
        [Authorize(Roles = "Empresa")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CompanyCreateRequest req)
        {
            var ownerId = CurrentUserId(User);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null) return NotFound("No tienes una empresa registrada.");

            if (!string.IsNullOrWhiteSpace(req.Name))
                company.Name = req.Name;

            await _context.SaveChangesAsync();
            return Ok(company);
        }

        // Eliminar mi empresa (implícita)
        [Authorize(Roles = "Empresa")]
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var ownerId = CurrentUserId(User);
            var company = await _context.Companies
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.OwnerUserId == ownerId);
            if (company == null) return NotFound("No tienes una empresa registrada.");

            // Evita borrar si quedan productos (o aplicar cascade si lo configuras)
            var hasProducts = company.Products != null && company.Products.Any();
            if (hasProducts)
                return BadRequest("Primero elimina/traspasa los productos de tu empresa.");

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
