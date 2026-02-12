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
            // 1. Verificación de seguridad manual (si no usas [Authorize])
            // Si estás usando Identity, lo mejor es poner [Authorize(Roles = "Admin")] arriba del método.
            // Pero si quieres mantener tu lógica actual:
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Obtener el proceso electoral activo
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success || procesoActivo.Data == null)
            {
                ModelState.AddModelError("", "⚠️ No hay un proceso electoral activo. Crea un proceso primero.");
                return View(candidato);
            }

            // 3. Asignar el ID del proceso al candidato
            candidato.ProcesoElectoralId = procesoActivo.Data.Id;

            // 👇 LA SOLUCIÓN AL BUG: Borramos el error de validación del ID
            ModelState.Remove("ProcesoElectoralId");

            // 4. Validar el resto del modelo
            if (!ModelState.IsValid)
            {
                // Si hay otros errores (ej: falta nombre), volvemos a la vista
                return View(candidato);
            }

            // 5. Enviar a la API
            var respuesta = await _apiService.PostAsync<Candidato>("Candidatos", candidato);

            if (respuesta.Success)
            {
                return RedirectToAction("Candidatos");
            }

            // Si la API devuelve error, lo mostramos
            ModelState.AddModelError("", $"Error al crear: {respuesta.Message}");
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