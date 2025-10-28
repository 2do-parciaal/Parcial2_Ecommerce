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

        // Público: lista empresas (con productos)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies
                .Include(c => c.Products)
                .ToListAsync();
            return Ok(companies);
        }

        // Público: empresa por id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null) return NotFound();
            return Ok(company);
        }

        // Solo Empresa puede crear su empresa
        [Authorize(Roles = "Empresa")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest("El nombre es obligatorio.");

            var ownerId = CurrentUserId(User);

            // (Opcional) bloquear que un mismo usuario cree más de una empresa
            var already = await _context.Companies.AnyAsync(c => c.OwnerUserId == ownerId);
            if (already) return BadRequest("Ya tienes una empresa registrada con este usuario.");

            var company = new Company { Name = req.Name, OwnerUserId = ownerId };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        // Solo Empresa puede editar su empresa
        [Authorize(Roles = "Empresa")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyCreateRequest req)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var ownerId = CurrentUserId(User);
            if (company.OwnerUserId != ownerId)
                return Forbid("Solo el dueño puede modificar esta empresa.");

            if (!string.IsNullOrWhiteSpace(req.Name))
                company.Name = req.Name;

            await _context.SaveChangesAsync();
            return Ok(company);
        }

        // Solo Empresa puede borrar su empresa
        [Authorize(Roles = "Empresa")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var ownerId = CurrentUserId(User);
            if (company.OwnerUserId != ownerId)
                return Forbid("Solo el dueño puede eliminar esta empresa.");

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
