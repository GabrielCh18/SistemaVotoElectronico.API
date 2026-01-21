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

        // ---------------------------------------------------------
        // 1. LISTAR CANDIDATOS (Con Seguridad 🔒)
        // ---------------------------------------------------------
        public async Task<IActionResult> Candidatos()
        {
            // --- CANDADO: Si no es admin, lo mandamos al Login ---
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // -----------------------------------------------------

            // Si pasa el candado, cargamos la lista
            var respuesta = await _apiService.GetListAsync<Candidato>("Candidatos/por-proceso/1");

            if (!respuesta.Success)
            {
                ViewBag.Error = "No se pudieron cargar los candidatos.";
                return View(new List<Candidato>());
            }

            return View(respuesta.Data);
        }

        // ---------------------------------------------------------
        // 2. CREAR CANDIDATO - VISTA (Con Seguridad 🔒)
        // ---------------------------------------------------------
        public IActionResult CrearCandidato()
        {
            // Candado
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ---------------------------------------------------------
        // 3. CREAR CANDIDATO - GUARDAR (Con Seguridad 🔒)
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            // Candado
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

            // Forzamos el proceso #1
            candidato.ProcesoElectoralId = 1;

            if (!ModelState.IsValid) return View(candidato);

            // Guardamos en la API
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

        // ---------------------------------------------------------
        // 4. ELIMINAR CANDIDATO (Con Seguridad 🔒)
        // ---------------------------------------------------------
        public async Task<IActionResult> Eliminar(int id)
        {
            // Candado
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

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