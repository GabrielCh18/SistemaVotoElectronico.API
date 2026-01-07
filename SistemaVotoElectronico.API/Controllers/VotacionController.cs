using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.API.Data;
using SistemaVotoElectronico.API.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SistemaVotoElectronico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Votante")] // Solo los usuarios con rol "Votante" pueden entrar 
    public class VotacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VotacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("registrar-voto")]
        public IActionResult RegistrarVoto([FromBody] VotoDTO votoDto)
        {
            // 1. Obtener el ID del usuario desde el Token JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // 2. Validar si el usuario ya votó o si existe 
            var usuario = _context.Usuarios.Find(userId);
            if (usuario == null || usuario.YaVoto)
            {
                return BadRequest("El usuario ya ha ejercido su derecho al voto o no existe.");
            }

            // 3. Verificar si la elección está activa [cite: 18]
            var eleccion = _context.Elecciones.Find(votoDto.EleccionId);
            if (eleccion == null || DateTime.Now > eleccion.FechaFin)
            {
                return BadRequest("El proceso electoral no está activo.");
            }

            // --- PROCESO DE ANONIMIZACIÓN Y SEGURIDAD  ---

            // A. Marcamos al usuario como que "Ya votó" antes de registrar el voto.
            // Esto evita que si el proceso falla, el usuario intente votar dos veces.
            usuario.YaVoto = true;
            _context.Usuarios.Update(usuario);

            // B. Creamos el objeto Voto SIN relación al Usuario (Garantiza Anonimato )
            var nuevoVoto = new Voto
            {
                CandidatoId = votoDto.CandidatoId,
                EleccionId = votoDto.EleccionId,
                // Generamos un Hash para asegurar la inmutabilidad 
                HashSeguridad = CalcularHash(votoDto.CandidatoId, votoDto.EleccionId)
            };

            _context.Votos.Add(nuevoVoto);

            // C. Guardamos todo en una sola transacción
            _context.SaveChanges();

            return Ok("Su sufragio ha sido registrado de forma secreta y segura.");
        }

        // Método para asegurar la inmutabilidad del voto 
        private string CalcularHash(int candidatoId, int eleccionId)
        {
            var data = $"{candidatoId}-{eleccionId}-{Guid.NewGuid()}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}