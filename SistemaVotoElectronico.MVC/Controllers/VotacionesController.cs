using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.MVC.Controllers
{
    // CANDADO DE SEGURIDAD
    [Authorize(Roles = "Admin")]
    public class VotacionesController : Controller
    {
        private readonly ApiService _apiService;

        public VotacionesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // LISTAR ELECCIONES
        public async Task<IActionResult> Index()
        {

            var response = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");

            if (!response.Success)
            {
                ViewBag.Error = response.Message;
                return View(new List<ProcesoElectoral>());
            }

            // Ordenamos: primero las activas, luego las más nuevas
            var listaOrdenada = response.Data
                .OrderByDescending(p => p.Activo)
                .ThenByDescending(p => p.FechaInicio)
                .ToList();

            return View(listaOrdenada);
        }

        // 2. ACTIVAR / CERRAR (Manejo de estados)
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string accion)
        {

            string endpoint = "";

            if (accion == "cerrar")
            {
                endpoint = $"ProcesosElectorales/cerrar/{id}";
            }
            else if (accion == "activar")
            {
                endpoint = $"ProcesosElectorales/activar/{id}";
            }

            var response = await _apiService.PostAsync<object>(endpoint, null);

            if (!response.Success)
            {
                TempData["Error"] = "No se pudo cambiar el estado: " + response.Message;
            }
            else
            {
                TempData["Mensaje"] = "Estado actualizado correctamente.";
            }

            return RedirectToAction("Index");
        }

        // METODOS CREATE (AGREGADOS PARA COMPLETAR)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcesoElectoral proceso)
        {
            if (ModelState.IsValid)
            {
                var response = await _apiService.PostAsync("ProcesosElectorales", proceso);
                if (response.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", response.Message);
            }
            return View(proceso);
        }
    }
}