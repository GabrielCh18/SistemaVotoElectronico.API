using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
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
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "⚠️ Por favor, ingrese un número de cédula.";
                return View("Index");
            }

            // CORRECCIÓN: Usamos PostWithResponseAsync
            // <LoQueEnvio, LoQueRecibo>
            // Enviamos un objeto vacío (new RespuestaCodigo) solo para cumplir con el requisito
            var response = await _apiService.PostWithResponseAsync<RespuestaCodigo, RespuestaCodigo>(
                $"Votantes/generar-codigo/{cedula}",
                new RespuestaCodigo()
            );

            if (response.Success && response.Data != null)
            {
                // AHORA SÍ FUNCIONA: Data ya es un objeto, no un string
                ViewBag.CodigoGenerado = response.Data.Codigo;
                ViewBag.NombreVotante = response.Data.Nombre;
                ViewBag.Mensaje = "✅ CÓDIGO GENERADO EXITOSAMENTE";
            }
            else
            {
                ViewBag.Error = "❌ " + response.Message;
            }

            return View("Index");
        }
    }

    // Clase para recibir la respuesta (DTO)
    public class RespuestaCodigo
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}