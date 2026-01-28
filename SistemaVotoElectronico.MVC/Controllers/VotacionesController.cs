using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class VotacionesController : Controller
    {
        private readonly ApiService _apiService;

        public VotacionesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // 1. LISTAR ELECCIONES
        public async Task<IActionResult> Index()
        {
            // Verificamos si es Admin (Seguridad)
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

            // Llamamos a la API usando el nombre que puso tu compañero: "ProcesosElectorales"
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
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

            string endpoint = "";

            // Tu API tiene endpoints específicos para esto
            if (accion == "cerrar")
            {
                endpoint = $"ProcesosElectorales/cerrar/{id}";
            }
            else if (accion == "activar")
            {
                // NOTA: Si tu API no tiene endpoint "activar", tendríamos que crear uno nuevo
                // o editar el objeto. Asumiré por ahora que tu compañero creó lógica para "activar".
                // Si no, avísame y lo ajustamos.
                endpoint = $"ProcesosElectorales/activar/{id}";
            }

            // Enviamos la petición (PostAsync requiere un cuerpo, mandamos null porque es solo comando)
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
    }
}