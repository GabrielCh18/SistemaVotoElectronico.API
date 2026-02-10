using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Candidatos()
        {
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ViewBag.Error = "⚠️ No hay un proceso electoral activo.";
                return View(new List<Candidato>());
            }
            var respuesta = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{procesoActivo.Data.Id}");
            return View(respuesta.Success ? respuesta.Data : new List<Candidato>());
        }

        [HttpGet]
        public IActionResult CrearCandidato() => View();

        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ViewBag.Error = "⚠️ No hay un proceso activo.";
                return View(candidato);
            }

            candidato.ProcesoElectoralId = procesoActivo.Data.Id;

            // 🔥 CORRECCIÓN: Limpieza de validación para que permita guardar
            ModelState.Remove("ProcesoElectoral");
            ModelState.Remove("Votos");

            if (!ModelState.IsValid) return View(candidato);

            var respuesta = await _apiService.PostAsync<Candidato>("Candidatos", candidato);
            if (respuesta.Success) return RedirectToAction("Candidatos");

            ViewBag.Error = "❌ Error: " + respuesta.Message;
            return View(candidato);
        }

        [HttpGet]
        public IActionResult CrearProceso() => View(new ProcesoElectoral { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddHours(3), Activo = true });

        [HttpPost]
        public async Task<IActionResult> CrearProceso(ProcesoElectoral proceso)
        {
            if (proceso.FechaInicio >= proceso.FechaFin)
            {
                ModelState.AddModelError("", "❌ La fecha inicio debe ser menor a la fin.");
                return View(proceso);
            }
            proceso.Activo = true;
            var response = await _apiService.PostAsync<ProcesoElectoral>("ProcesosElectorales", proceso);
            if (response.Success) return RedirectToAction("Candidatos");
            return View(proceso);
        }
    }
}