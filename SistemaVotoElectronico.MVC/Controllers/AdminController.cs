using Microsoft.AspNetCore.Mvc;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool NoEsAdmin() =>
            HttpContext.Session.GetString("UsuarioAdmin") == null;

        // ---------------------------------------------------------
        // 1️⃣ LISTAR CANDIDATOS DEL PROCESO ACTIVO REAL
        // ---------------------------------------------------------
        public async Task<IActionResult> Candidatos()
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            // 🔥 AHORA usamos el endpoint correcto
            var procesoActivo = await _apiService
                .GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ViewBag.Error = "⚠️ No hay un proceso electoral activo.";
                return View(new List<Candidato>());
            }

            var respuesta = await _apiService
                .GetListAsync<Candidato>($"Candidatos/por-proceso/{procesoActivo.Data.Id}");

            return View(respuesta.Success ? respuesta.Data : new List<Candidato>());
        }

        // ---------------------------------------------------------
        // 2️⃣ CREAR PROCESO ELECTORAL (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public IActionResult CrearProceso()
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            return View(new ProcesoElectoral
            {
                FechaInicio = DateTime.Now.AddMinutes(5),
                FechaFin = DateTime.Now.AddHours(3),
                Activo = true
            });
        }

        // ---------------------------------------------------------
        // 2️⃣ CREAR PROCESO ELECTORAL (POST)
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CrearProceso(ProcesoElectoral proceso)
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            if (proceso.FechaInicio >= proceso.FechaFin)
            {
                ModelState.AddModelError("", "❌ La fecha de inicio debe ser menor a la fecha de cierre.");
                return View(proceso);
            }

            // 🔒 Cerrar procesos activos anteriores
            var procesos = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");

            var activos = procesos.Data?.Where(p => p.Activo).ToList();

            if (activos != null)
            {
                foreach (var p in activos)
                {
                    await _apiService.PostAsync<object>($"ProcesosElectorales/cerrar/{p.Id}", null);
                }
            }

            proceso.Activo = true;

            var response = await _apiService.PostAsync<ProcesoElectoral>("ProcesosElectorales", proceso);

            if (response.Success)
            {
                TempData["Mensaje"] = "✅ Proceso electoral creado correctamente.";
                return RedirectToAction("Candidatos");
            }

            ModelState.AddModelError("", response.Message);
            return View(proceso);
        }

        // ---------------------------------------------------------
        // 3️⃣ CREAR CANDIDATO (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public IActionResult CrearCandidato()
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ---------------------------------------------------------
        // 3️⃣ CREAR CANDIDATO (POST)
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            var procesoActivo = await _apiService
                .GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ModelState.AddModelError("", "⚠️ No hay un proceso electoral activo.");
                return View(candidato);
            }

            candidato.ProcesoElectoralId = procesoActivo.Data.Id;

            if (!ModelState.IsValid)
                return View(candidato);

            var respuesta = await _apiService.PostAsync<Candidato>("Candidatos", candidato);

            if (respuesta.Success)
                return RedirectToAction("Candidatos");

            ModelState.AddModelError("", respuesta.Message);
            return View(candidato);
        }

        // ---------------------------------------------------------
        // 4️⃣ ELIMINAR CANDIDATO
        // ---------------------------------------------------------
        public async Task<IActionResult> Eliminar(int id)
        {
            if (NoEsAdmin())
                return RedirectToAction("Login", "Account");

            var respuesta = await _apiService.DeleteAsync($"Candidatos/{id}");

            if (!respuesta.Success)
                TempData["Error"] = respuesta.Message;

            return RedirectToAction("Candidatos");
        }
    }
}
