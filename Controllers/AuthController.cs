using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Auth;
using Parcial2_Ecommerce.Services;

namespace Parcial2_Ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Registro: público y NO devuelve token
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email y password son obligatorios.");

            if (_context.Users.Any(u => u.Email == req.Email))
                return BadRequest("El correo ya está registrado.");

            var user = new User
            {
                Name = req.Name,
                Email = req.Email,
                Password = req.Password,
                Role = string.IsNullOrWhiteSpace(req.Role) ? "Cliente" : req.Role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Ya NO devolvemos token aquí
            return Ok(new { message = "Usuario registrado correctamente." });
        }

        // Login: público y devuelve token
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email y password son obligatorios.");

            var user = _context.Users.FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
            if (user == null) return Unauthorized("Credenciales inválidas.");

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token, role = user.Role });
        }
    }
}
