using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parcial2_Ecommerce.Data;
using Parcial2_Ecommerce.Models;
using Parcial2_Ecommerce.Models.Auth;
using Parcial2_Ecommerce.Services;
using Microsoft.AspNetCore.Identity; // <- PasswordHasher

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
                // Guardaremos el HASH, no el texto plano
                Password = "", 
                Role = string.IsNullOrWhiteSpace(req.Role) ? "Cliente" : req.Role
            };

            // Hash de la contraseña
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, req.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Usuario registrado correctamente." });
        }

        // Login: público y devuelve token
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email y password son obligatorios.");

            var user = _context.Users.FirstOrDefault(u => u.Email == req.Email);
            if (user == null) return Unauthorized("Credenciales inválidas.");

            var hasher = new PasswordHasher<User>();
            var verification = hasher.VerifyHashedPassword(user, user.Password, req.Password);

            if (verification == PasswordVerificationResult.Failed)
                return Unauthorized("Credenciales inválidas.");

            // (Opcional) Si requiere rehash, actualizamos el hash
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = hasher.HashPassword(user, req.Password);
                _context.SaveChanges();
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new { token, role = user.Role });
        }
    }
}
