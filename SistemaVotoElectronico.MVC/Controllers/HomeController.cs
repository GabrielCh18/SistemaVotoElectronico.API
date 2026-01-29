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

        public HomeController(ILogger<HomeController> logger, ApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // 👇 ESTE ES EL MÉTODO ÚNICO (FUSIONADO)
        public async Task<IActionResult> Index(string? cedula)
        {
            // ---------------------------------------------------------
            // PARTE 1: VERIFICAR ESTADO DE ELECCIÓN (Para el botón)
            // ---------------------------------------------------------
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            bool hayEleccionEnCurso = (procesoActivo.Success && procesoActivo.Data != null);

            if (hayEleccionEnCurso)
            {
                ViewBag.MostrarResultados = false;
                ViewBag.Mensaje = "Las urnas están abiertas. ¡Acércate a votar!";
            }
            else
            {
                // Si no hay activa, miramos si hubo alguna antes para habilitar el botón
                var historial = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
                bool hayHistorial = historial.Success && historial.Data != null && historial.Data.Any();

                ViewBag.MostrarResultados = hayHistorial;
                ViewBag.Mensaje = "Proceso electoral finalizado.";
            }

            // ---------------------------------------------------------
            // PARTE 2: LÓGICA DEL BUSCADOR (Si escribieron cédula)
            // ---------------------------------------------------------
            if (!string.IsNullOrEmpty(cedula))
            {
                var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

                if (response.Success && response.Data != null)
                {
                    return View(response.Data); // Retorna vista CON el votante encontrado
                }
                else
                {
                    ViewBag.Error = "⚠️ No encontramos esa cédula en el padrón electoral.";
                    ViewBag.CedulaBuscada = cedula;
                }
            }

            // Si no buscaron nada, o no encontraron, retornamos la vista normal
            return View();
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