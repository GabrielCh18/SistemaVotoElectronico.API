using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.MVC.Models;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos;
using System.Diagnostics;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _apiService;

        // Inyectamos el ApiService para poder consultar datos
        public HomeController(ILogger<HomeController> logger, ApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(string? cedula)
        {
            // --- LÓGICA DE VISIBILIDAD DE RESULTADOS ---

            // 1. Preguntamos a la API si hay un proceso activo
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            bool hayEleccionEnCurso = (procesoActivo.Success && procesoActivo.Data != null);

            // ✅ CORRECCIÓN: Quitamos la excepción del Admin.
            // Ahora la regla es estricta: Solo se ven resultados si NO hay elección en curso.
            ViewBag.MostrarResultados = !hayEleccionEnCurso;

            // (El resto del código de búsqueda sigue igual...)
            if (string.IsNullOrEmpty(cedula))
            {
                return View();
            }

            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

            if (response.Success)
            {
                return View(response.Data);
            }
            else
            {
                ViewBag.Error = "⚠️ No encontramos esa cédula en el padrón electoral.";
                ViewBag.CedulaBuscada = cedula;
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}