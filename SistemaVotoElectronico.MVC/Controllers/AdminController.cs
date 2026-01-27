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

        // 🔒 Validar sesión
        private bool NoEsAdmin() => HttpContext.Session.GetString("UsuarioAdmin") == null;

        // ---------------------------------------------------------
        // 1. LISTAR CANDIDATOS
        // ---------------------------------------------------------
        public async Task<IActionResult> Candidatos()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // Buscamos el proceso activo para filtrar candidatos
            var procesos = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
            var activo = procesos.Data?.FirstOrDefault(p => p.Activo);

            if (activo == null)
            {
                ViewBag.Error = "No hay una elección activa. Crea una primero.";
                return View(new List<Candidato>());
            }

            var respuesta = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{activo.Id}");
            return View(respuesta.Success ? respuesta.Data : new List<Candidato>());
        }

        // ---------------------------------------------------------
        // 2. CREAR PROCESO ELECTORAL (¡NUEVO!)
        // ---------------------------------------------------------
        public IActionResult CrearProceso()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearProceso(ProcesoElectoral proceso)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                proceso.Activo = true; // Nace activo
                proceso.FechaInicio = DateTime.Now;

                var response = await _apiService.PostAsync("ProcesosElectorales", proceso);
                if (response.Success)
                {
                    TempData["Mensaje"] = "✅ Elección creada con éxito.";
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", response.Message);
            }
            return View(proceso);
        }

        // ---------------------------------------------------------
        // 3. CREAR CANDIDATO (Con detección automática de proceso)
        // ---------------------------------------------------------
        public IActionResult CrearCandidato()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // 1. Buscamos cuál es la elección actual
            var procesos = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
            var activo = procesos.Data?.FirstOrDefault(p => p.Activo);

            if (activo != null)
            {
                candidato.ProcesoElectoralId = activo.Id; // Asignamos ID real
            }
            else
            {
                ModelState.AddModelError("", "⚠️ No hay elección activa. Crea un Proceso primero.");
                return View(candidato);
            }

            if (!ModelState.IsValid) return View(candidato);

            var respuesta = await _apiService.PostAsync("Candidatos", candidato);

            if (respuesta.Success) return RedirectToAction("Candidatos");

            ViewBag.Error = "Error: " + respuesta.Message;
            return View(candidato);
        }

        // ---------------------------------------------------------
        // 4. ELIMINAR CANDIDATO
        // ---------------------------------------------------------
        public async Task<IActionResult> Eliminar(int id)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            var respuesta = await _apiService.DeleteAsync($"Candidatos/{id}");
            if (!respuesta.Success) TempData["Error"] = respuesta.Message;

            return RedirectToAction("Candidatos");
        }
    }
}