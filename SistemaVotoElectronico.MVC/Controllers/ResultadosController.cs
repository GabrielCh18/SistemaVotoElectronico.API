using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos; // <--- ¡Este es el cambio importante!

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class ResultadosController : Controller
    {
        private readonly ApiService _apiService;

        public ResultadosController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Pedimos los resultados de la elección #1
            // Nota: Asegúrate de tener la clase 'ResumenGeneral' en tus modelos del MVC
            var respuesta = await _apiService.GetAsync<ResumenGeneral>("Resultados/1");

            if (respuesta.Success)
            {
                return View(respuesta.Data);
            }

            // Si falla, mostramos error
            ViewBag.Error = respuesta.Message;
            return View(new ResumenGeneral());
        }
    }
}