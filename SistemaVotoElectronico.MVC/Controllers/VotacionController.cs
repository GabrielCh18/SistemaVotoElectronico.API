using Microsoft.AspNetCore.Mvc;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class VotacionController : Controller
    {
        private readonly ApiService _apiService;

        public VotacionController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // 1️⃣ LOGIN
        public IActionResult Index()
        {
            return View();
        }

        // 2️⃣ VALIDAR INGRESO
        [HttpPost]
        public async Task<IActionResult> Ingresar(string cedula, string codigo)
        {
            // 🔍 Consultar proceso activo
            var procesoActivo = await _apiService
                .GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            if (!procesoActivo.Success)
            {
                ViewBag.Error = "🔥 Error al consultar el proceso electoral.";
                return View("Index");
            }

            if (procesoActivo.Data == null)
            {
                var procesos = await _apiService
                    .GetListAsync<ProcesoElectoral>("ProcesosElectorales");

                if (procesos.Data != null && procesos.Data.Any())
                {
                    var ultimo = procesos.Data
                        .OrderByDescending(p => p.FechaInicio)
                        .First();

                    var ahora = DateTime.UtcNow;

                    if (ahora < ultimo.FechaInicio)
                    {
                        ViewBag.Error =
                            $"⏰ La votación inicia el {ultimo.FechaInicio:dd/MM/yyyy} a las {ultimo.FechaInicio:HH:mm}.";
                    }
                    else
                    {
                        ViewBag.Error = "🚫 El proceso electoral ya fue cerrado.";
                    }
                }
                else
                {
                    ViewBag.Error = "❌ No existen procesos electorales registrados.";
                }

                return View("Index");
            }

            // 🔹 Validar votante (SOLO existencia)
            var votante = await _apiService
                .GetAsync<Votante>($"Votantes/buscar/{cedula}");

            if (!votante.Success)
            {
                ViewBag.Error = $"🔥 ERROR DE CONEXIÓN: {votante.Message}";
                return View("Index");
            }

            if (votante.Data == null)
            {
                ViewBag.Error = "❌ Votante no encontrado.";
                return View("Index");
            }

            // ⚠️ YA NO SE VALIDA YaVoto AQUÍ

            // Guardar datos para el flujo
            TempData["Cedula"] = cedula;
            TempData["Codigo"] = codigo;
            TempData["ProcesoId"] = procesoActivo.Data.Id;

            return RedirectToAction("Papeleta");
        }

        // 3️⃣ PAPELETA
        public async Task<IActionResult> Papeleta()
        {
            if (TempData["Cedula"] == null)
                return RedirectToAction("Index");

            TempData.Keep();

            int procesoId = (int)TempData["ProcesoId"];

            var candidatos = await _apiService
                .GetListAsync<Candidato>($"Candidatos/por-proceso/{procesoId}");

            return View(candidatos.Data);
        }

        // 4️⃣ VOTAR
        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId)
        {
            var cedula = TempData["Cedula"]?.ToString();
            var codigo = TempData["Codigo"]?.ToString();
            int procesoId = (int)TempData["ProcesoId"];

            string url =
                $"Votos/registrar-voto?cedula={cedula}&codigo={codigo}&candidatoId={candidatoId}&procesoElectoralId={procesoId}";

            var respuesta = await _apiService.PostAsync<object>(url, null);

            if (respuesta.Success)
                return RedirectToAction("Exito");

            ViewBag.Error = respuesta.Message;

            var candidatos = await _apiService
                .GetListAsync<Candidato>($"Candidatos/por-proceso/{procesoId}");

            return View("Papeleta", candidatos.Data);
        }

        public IActionResult Exito()
        {
            return View();
        }
    }
}