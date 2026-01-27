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
            // 1. Preguntar a la API qué elección está activa
            var procesosResp = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");

            // Buscamos el activo
            var activo = procesosResp.Data?.FirstOrDefault(p => p.Activo);

            if (activo != null)
            {
                // 2. Pedimos resultados de ESA elección específica
                var respuesta = await _apiService.GetAsync<ResumenGeneral>($"Resultados/{activo.Id}");

                if (respuesta.Success)
                {
                    return View(respuesta.Data);
                }
            }

            // Si no hay elección o falló
            ViewBag.Error = "No hay resultados disponibles en este momento.";
            return View(new ResumenGeneral());
        }
    }
}