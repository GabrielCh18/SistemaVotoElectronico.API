using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.API.Data;
using SistemaVotoElectronico.API.Models;

namespace SistemaVotoElectronico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación JWT para acceder
    public class VotacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VotacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar-voto")]
        public IActionResult RegistrarVoto([FromBody] VotoDTO voto)
        {
            // Aquí se implementará la lógica de:
            // 1. Validar que el usuario no haya votado antes.
            // 2. Proceso de anonimización y cifrado.
            // 3. Registro inmutable en la base de datos.

            return Ok("Voto registrado de forma segura y anónima.");
        }
    }
}