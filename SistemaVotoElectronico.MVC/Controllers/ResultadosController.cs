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

        public async Task<IActionResult> Index(int? procesoId)
        {
            ProcesoElectoral proceso = null;

            // CASO 1: Vienes del Historial (Traes ID específico)
            if (procesoId.HasValue)
            {
                var result = await _apiService.GetAsync<ProcesoElectoral>($"ProcesosElectorales/{procesoId}");
                if (result.Success) proceso = result.Data;
            }
            else
            {
                // CASO 2: Vienes del Menú o del Usuario (No traes ID)

                // A. Intentamos buscar uno ACTIVO
                var activoResp = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

                if (activoResp.Success && activoResp.Data != null)
                {
                    proceso = activoResp.Data;
                }
                else
                {
                    // B. 🔥 EL TRUCO: Si no hay activo, buscamos el ÚLTIMO CERRADO
                    var listaResp = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");

                    if (listaResp.Success && listaResp.Data != null)
                    {
                        // Ordenamos por fecha fin descendente (el más reciente primero) y tomamos el 1ro
                        proceso = listaResp.Data
                            .OrderByDescending(p => p.FechaFin)
                            .FirstOrDefault();
                    }
                }
            }

            if (proceso == null)
            {
                ViewBag.Error = "No se encontraron elecciones registradas en el sistema.";
                return View(new ResumenGeneral()); // Modelo vacío para no romper la vista
            }

            // Guardamos el nombre para mostrarlo en la vista
            ViewBag.NombreProceso = $"Elecciones del {proceso.FechaInicio:dd/MM/yyyy}";

            // Pedimos los resultados al API usando el ID que encontramos (sea activo, cerrado o especifico)
            var respuesta = await _apiService.GetAsync<ResumenGeneral>($"Resultados/{proceso.Id}");

            if (respuesta.Success)
            {
                return View(respuesta.Data);
            }

            ViewBag.Error = "No se pudieron cargar los datos del conteo.";
            return View(new ResumenGeneral());
        }
    }
}