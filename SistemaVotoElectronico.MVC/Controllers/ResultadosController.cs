using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // SOLO DEBE HABER ESTE MÉTODO INDEX (El completo)
        public async Task<IActionResult> Index(int? procesoId, int? provinciaId, int? cantonId, int? parroquiaId)
        {
            ProcesoElectoral proceso = null;

            // 1. LÓGICA PARA ELEGIR EL PROCESO (Activo o por ID)
            if (procesoId.HasValue)
            {
                var result = await _apiService.GetAsync<ProcesoElectoral>($"ProcesosElectorales/{procesoId}");
                if (result.Success) proceso = result.Data;
            }
            else
            {
                var activoResp = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
                if (activoResp.Success && activoResp.Data != null)
                {
                    proceso = activoResp.Data;
                }
                else
                {
                    // Si no hay activo, busca el último cerrado
                    var listaResp = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
                    if (listaResp.Success && listaResp.Data != null)
                        proceso = listaResp.Data.OrderByDescending(p => p.FechaFin).FirstOrDefault();
                }
            }

            // 2. CARGAR COMBOS DE FILTRO
            var provsResp = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provsResp.Data ?? new List<Provincia>(), "Id", "Nombre", provinciaId);

            ViewBag.ProvinciaId = provinciaId;
            ViewBag.CantonId = cantonId;
            ViewBag.ParroquiaId = parroquiaId;
            ViewBag.ProcesoId = proceso?.Id;

            if (proceso == null)
            {
                ViewBag.Error = "No hay elecciones registradas.";
                return View(new ResumenGeneral());
            }

            ViewBag.NombreProceso = proceso.Nombre; // Muestra el nombre real

            // 3. PEDIR RESULTADOS A LA API
            string url = $"Resultados/{proceso.Id}?dummy=1";
            if (provinciaId.HasValue) url += $"&provinciaId={provinciaId}";
            if (cantonId.HasValue) url += $"&cantonId={cantonId}";
            if (parroquiaId.HasValue) url += $"&parroquiaId={parroquiaId}";

            var respuesta = await _apiService.GetAsync<ResumenGeneral>(url);

            if (respuesta.Success) return View(respuesta.Data);

            ViewBag.Error = "No se pudieron cargar los datos.";
            return View(new ResumenGeneral());
        }
    }
}