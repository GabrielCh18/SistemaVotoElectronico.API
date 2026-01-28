using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos;

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
            // 🔥 1. Obtener el proceso ACTIVO REAL (con validación de horario)
            var procesoResp = await _apiService
                .GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoResp.Success || procesoResp.Data == null)
            {
                ViewBag.Error = "No hay un proceso electoral activo en este momento.";
                return View(new ResumenGeneral());
            }

            // 🔥 2. Pedimos resultados de ESE proceso
            var respuesta = await _apiService
                .GetAsync<ResumenGeneral>($"Resultados/{procesoResp.Data.Id}");

            if (respuesta.Success)
            {
                return View(respuesta.Data);
            }

            ViewBag.Error = "No hay resultados disponibles en este momento.";
            return View(new ResumenGeneral());
        }
    }
}
