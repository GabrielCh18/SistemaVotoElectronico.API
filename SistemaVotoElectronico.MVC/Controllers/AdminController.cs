using Microsoft.AspNetCore.Mvc;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // 1. LISTAR CANDIDATOS
        // GET: Admin/Candidatos
        public async Task<IActionResult> Candidatos()
        {
            // Pedimos la lista a la API (Asumimos proceso #1 por ahora)
            var respuesta = await _apiService.GetListAsync<Candidato>("Candidatos/por-proceso/1");

            if (!respuesta.Success)
            {
                ViewBag.Error = "No se pudieron cargar los candidatos.";
                return View(new List<Candidato>());
            }

            return View(respuesta.Data);
        }

        // 2. PANTALLA DE CREAR (GET)
        public IActionResult CrearCandidato()
        {
            return View();
        }

        // 3. GUARDAR CANDIDATO (POST)
        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            // Forzamos algunos datos por defecto para facilitar
            candidato.ProcesoElectoralId = 1;

            if (!ModelState.IsValid) return View(candidato);

            // Enviamos a la API
            var respuesta = await _apiService.PostAsync("Candidatos", candidato);

            if (respuesta.Success)
            {
                return RedirectToAction("Candidatos");
            }
            else
            {
                ViewBag.Error = "Error al guardar: " + respuesta.Message;
                return View(candidato);
            }
        }

        // 4. ELIMINAR CANDIDATO
        public async Task<IActionResult> Eliminar(int id)
        {
            // Ojo: Necesitarás crear este endpoint en tu ApiService si no existe un DeleteAsync,
            // pero por ahora usemos Post o Get si tu API lo permite, o dejémoslo pendiente.
            // Para simplificar hoy, solo haremos Listar y Crear.
            return RedirectToAction("Candidatos");
        }
    }
}