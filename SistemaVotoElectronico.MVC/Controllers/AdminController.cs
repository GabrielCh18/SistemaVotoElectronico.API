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

        // 1️⃣ LISTAR CANDIDATOS DEL PROCESO ACTIVO REAL
        public async Task<IActionResult> Candidatos()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ViewBag.Error = "⚠️ No hay un proceso electoral activo.";
                return View(new List<Candidato>());
            }

            var respuesta = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{procesoActivo.Data.Id}");

            return View(respuesta.Success ? respuesta.Data : new List<Candidato>());
        }

        // 2️⃣ CREAR PROCESO ELECTORAL (GET)
        [HttpGet]
        public IActionResult CrearProceso()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            return View(new ProcesoElectoral
            {
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddHours(3),
                Activo = true
            });
        }

        // 2️⃣ CREAR PROCESO ELECTORAL (POST)
        [HttpPost]
        public async Task<IActionResult> CrearProceso(ProcesoElectoral proceso)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            if (proceso.FechaInicio >= proceso.FechaFin)
            {
                ModelState.AddModelError("", "❌ La fecha de inicio debe ser menor a la fecha de cierre.");
                return View(proceso);
            }

            // 🔒 Cerrar procesos activos anteriores automáticamente
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

        // 3️⃣ CREAR CANDIDATO (GET)
        [HttpGet]
        public IActionResult CrearCandidato()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // 3️⃣ CREAR CANDIDATO (POST)
        [HttpPost]
        public async Task<IActionResult> CrearCandidato(Candidato candidato)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ModelState.AddModelError("", "⚠️ No hay un proceso electoral activo. Crea un proceso primero.");
                return View(candidato);
            }

            candidato.ProcesoElectoralId = procesoActivo.Data.Id;

            if (!ModelState.IsValid) return View(candidato);

            var respuesta = await _apiService.PostAsync<Candidato>("Candidatos", candidato);

            if (respuesta.Success) return RedirectToAction("Candidatos");

            ModelState.AddModelError("", respuesta.Message);
            return View(candidato);
        }

        // 4️⃣ ELIMINAR CANDIDATO
        public async Task<IActionResult> Eliminar(int id)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            var respuesta = await _apiService.DeleteAsync($"Candidatos/{id}");
            if (!respuesta.Success) TempData["Error"] = respuesta.Message;

            return RedirectToAction("Candidatos");
        }

        // =========================================================
        // 🆕 FUNCIONES NUEVAS PARA EL HISTORIAL Y ELIMINAR
        // =========================================================

        // 5️⃣ HISTORIAL DE PROCESOS (Para ver viejos y borrar)
        [HttpGet]
        public async Task<IActionResult> Historial()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            var response = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
            // Ordenamos: El más reciente primero
            var lista = response.Data?.OrderByDescending(p => p.FechaInicio).ToList() ?? new List<ProcesoElectoral>();

            return View(lista);
        }

        // 6️⃣ ELIMINAR PROCESO (Borrado completo)
        public async Task<IActionResult> EliminarProceso(int id)
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // Llamamos al API para borrar
            await _apiService.DeleteAsync($"ProcesosElectorales/{id}");

            // Recargamos el historial
            return RedirectToAction("Historial");
        }

        // 7️⃣ FINALIZAR ELECCIÓN ACTIVA (Cierra y lleva a resultados)
        public async Task<IActionResult> FinalizarActual()
        {
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // 1. Buscamos el activo
            var activo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!activo.Success || activo.Data == null)
                return RedirectToAction("Historial"); // Si no hay activo, vamos al historial

            // 2. Lo cerramos
            await _apiService.PostAsync<object>($"ProcesosElectorales/cerrar/{activo.Data.Id}", null);

            // 3. Redirigimos a Resultados enviando el ID específico
            return RedirectToAction("Index", "Resultados", new { procesoId = activo.Data.Id });
        }
    }
}