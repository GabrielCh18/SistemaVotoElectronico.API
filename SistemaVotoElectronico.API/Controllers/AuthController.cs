using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaVotoElectronico.API.Models;
using SistemaVotoElectronico.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaVotoElectronico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            // 1. Buscar al usuario en la base de datos por su email [cite: 13]
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email.ToLower() == login.Email.ToLower());

            // 2. Validar credenciales (En un entorno real, aquí se compara el Hash) [cite: 13]
            if (usuario == null || usuario.Password != login.Password)
            {
                return Unauthorized("Correo o contraseña incorrectos");
            }

            // 3. Crear los "Claims" (Datos que viajan dentro del Token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol) // Aquí va: Administrador, Votante o Candidato [cite: 12, 15]
            };

            // 4. Generar la llave de seguridad (debe ser la misma que pusimos en Program.cs) 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EstaEsMiClaveSuperSecretaDe123456"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Crear el Token JWT 
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8), // El token dura 8 horas
                signingCredentials: creds
            );

            // 6. Devolver el Token al usuario
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}