using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    // [Authorize(Roles = "JefeMesa")] // Descomenta si usas roles
    public class JefeMesaController : Controller
    {
        private readonly ApiService _apiService;

        public JefeMesaController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generar(string cedula)
        {
            // Enviamos la petición a la API
            var response = await _apiService.PostAsync<dynamic>($"api/Votantes/generar-codigo/{cedula}", null);

            if (response.Success)
            {
                // Leemos el código que devolvió la API (usando Newtonsoft.Json si es necesario parsear mejor)
                // Aquí asumimos que response.Data trae el objeto JSON
                ViewBag.CodigoGenerado = response.Data.ToString();
                ViewBag.Mensaje = "✅ CÓDIGO GENERADO. Entréguelo al votante.";
            }
            else
            {
                ViewBag.Error = "❌ Error: " + response.Message;
            }

            return View("Index");
        }
    }
}