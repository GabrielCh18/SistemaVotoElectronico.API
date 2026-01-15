using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVotoElectronico.API;
using SistemaVotoElectronico.API.Models;

namespace SistemaVotoElectronico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JefeJuntaController : ControllerBase
    {
        // 1. Declarar la variable de la base de datos
        private readonly ApplicationDbContext _context;

        // 2. Crear el Constructor (ESTO TE FALTA)
        public JefeJuntaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("activar/{cedula}")]
        public IActionResult Activar(string cedula)
        {
            // Ahora '_context' ya no saldrá en rojo
            var user = _context.Usuarios.FirstOrDefault(u => u.Cedula == cedula);

            if (user == null || user.YaVoto) return BadRequest("No habilitado");

            user.CodigoAcceso = new Random().Next(1000, 9999).ToString();
            _context.SaveChanges();

            return Ok(new { Codigo = user.CodigoAcceso });
        }
    }
}