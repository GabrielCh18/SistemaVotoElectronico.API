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

        // 1. LISTAR CANDIDATOS (PROTEGIDO)
        public async Task<IActionResult> Candidatos()
        {
            // --- CANDADO 🔒 ---
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
            {
                return RedirectToAction("Login", "Account"); // ¡Fuera de aquí!
            }
            // ------------------

            // Pedimos la lista a la API (Asumimos proceso #1)
            var respuesta = await _apiService.GetListAsync<Candidato>("Candidatos/por-proceso/1");

            if (!respuesta.Success)
            {
                ViewBag.Error = "No se pudieron cargar los candidatos.";
                return View(new List<Candidato>());
            }

            return View(respuesta.Data);
        }

        // 2. PANTALLA DE CREAR (PROTEGIDO)
        public IActionResult CrearCandidato()
        {
            // --- CANDADO 🔒 ---
            if (HttpContext.Session.GetString("UsuarioAdmin") == null) return RedirectToAction("Login", "Account");

            return View();
        }

        // 3. GUARDAR CANDIDATO (PROTEGIDO)
        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            // --- CANDADO 🔒 ---
            if (HttpContext.Session.GetString("UsuarioAdmin") == null) return RedirectToAction("Login", "Account");

            // Forzamos el proceso #1
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

        // 4. ELIMINAR CANDIDATO (PROTEGIDO Y FUNCIONAL)
        public async Task<IActionResult> Eliminar(int id)
        {
            // --- CANDADO 🔒 ---
            if (HttpContext.Session.GetString("UsuarioAdmin") == null) return RedirectToAction("Login", "Account");

            // Llamamos a la API para borrar
            var respuesta = await _apiService.DeleteAsync($"Candidatos/{id}");

            if (!respuesta.Success)
            {
                TempData["Error"] = "No se pudo eliminar: " + respuesta.Message;
            }

            return RedirectToAction("Candidatos");
        }
    }
}