using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CompaniesController(AppDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Companies.ToList());

        [HttpPost]
        public IActionResult Create(Company company)
        {
            _context.Companies.Add(company);
            _context.SaveChanges();
            return Ok(company);
        }
    }
}