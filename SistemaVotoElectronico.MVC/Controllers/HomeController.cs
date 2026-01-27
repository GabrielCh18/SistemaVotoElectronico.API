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
            // 1. Si no escribieron nada, mostramos la página limpia solo con el buscador
            if (string.IsNullOrEmpty(cedula))
            {
                return View();
            }

            // 2. Si escribieron una cédula, preguntamos a la API
            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

            if (response.Success)
            {
                // ¡Lo encontramos! Mandamos los datos a la vista
                return View(response.Data);
            }
            else
            {
                // No existe o hubo error
                ViewBag.Error = "⚠️ No encontramos esa cédula en el padrón electoral.";
                ViewBag.CedulaBuscada = cedula; // Para mantener lo que escribió
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