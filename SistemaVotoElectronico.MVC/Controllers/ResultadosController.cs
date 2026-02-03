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

        public async Task<IActionResult> Index(int? procesoId, int? provinciaId, int? cantonId, int? parroquiaId)
        {
            ProcesoElectoral proceso = null;

            //  LÓGICA PARA ELEGIR EL PROCESO 
            if (procesoId.HasValue)
            {
                // Si vienes del Historial
                var result = await _apiService.GetAsync<ProcesoElectoral>($"ProcesosElectorales/{procesoId}");
                if (result.Success) proceso = result.Data;
            }
            else
            {
                // Si vienes directo (Buscamos Activo o Último Cerrado)
                var activoResp = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
                if (activoResp.Success && activoResp.Data != null)
                {
                    proceso = activoResp.Data;
                }
                else
                {
                    var listaResp = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
                    if (listaResp.Success && listaResp.Data != null)
                    {
                        proceso = listaResp.Data.OrderByDescending(p => p.FechaFin).FirstOrDefault();
                    }
                }
            }

            //  CARGAMOS LAS PROVINCIAS PARA EL FILTRO 
            var provsResp = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provsResp.Data ?? new List<Provincia>(), "Id", "Nombre", provinciaId);

            // Guardamos selección actual para que JavaScript sepa qué mostrar
            ViewBag.ProvinciaId = provinciaId;
            ViewBag.CantonId = cantonId;
            ViewBag.ParroquiaId = parroquiaId;
            ViewBag.ProcesoId = proceso?.Id;

            if (proceso == null)
            {
                ViewBag.Error = "No se encontraron elecciones registradas.";
                return View(new ResumenGeneral());
            }

            ViewBag.NombreProceso = $"Elecciones del {proceso.FechaInicio:dd/MM/yyyy}";

            //  LLAMADA AL API CON FILTROS 
            string url = $"Resultados/{proceso.Id}?dummy=1"; 
            if (provinciaId.HasValue) url += $"&provinciaId={provinciaId}";
            if (cantonId.HasValue) url += $"&cantonId={cantonId}";
            if (parroquiaId.HasValue) url += $"&parroquiaId={parroquiaId}";

            var respuesta = await _apiService.GetAsync<ResumenGeneral>(url);

            if (respuesta.Success) return View(respuesta.Data);

            ViewBag.Error = "No se pudieron cargar los datos del conteo.";
            return View(new ResumenGeneral());
        }
    }
}