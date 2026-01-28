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

        // 1️⃣ PANTALLA DE INGRESO
        public IActionResult Index()
        {
            HttpContext.Session.Clear(); // Limpiamos sesión al entrar
            return View();
        }

        // 2️⃣ VALIDAR CÉDULA Y CÓDIGO
        [HttpPost]
        public async Task<IActionResult> Ingresar(string cedula, string codigo)
        {
            if (string.IsNullOrEmpty(cedula) || string.IsNullOrEmpty(codigo))
            {
                ViewBag.Error = "⚠️ Ingresa cédula y código.";
                return View("Index");
            }

            // Consultar proceso
            var proceso = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            if (!proceso.Success || proceso.Data == null)
            {
                ViewBag.Error = "❌ No hay votaciones activas.";
                return View("Index");
            }

            // Consultar Votante
            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");
            if (!response.Success || response.Data == null)
            {
                ViewBag.Error = "❌ Cédula no encontrada.";
                return View("Index");
            }

            var votante = response.Data;

            // 🔐 VALIDACIÓN DEL TOKEN (Aquí comparamos con lo que mandó la API)
            if (votante.Token != codigo)
            {
                ViewBag.Error = "⛔ Código incorrecto o caducado.";
                return View("Index");
            }

            if (votante.YaVoto)
            {
                ViewBag.Error = "⚠️ Usted ya votó en este proceso.";
                return View("Index");
            }

            // ✅ GUARDAR EN SESIÓN (Seguro)
            HttpContext.Session.SetString("Cedula", cedula);
            HttpContext.Session.SetString("Codigo", codigo);
            HttpContext.Session.SetInt32("ProcesoId", proceso.Data.Id);

            return RedirectToAction("Papeleta");
        }

        // 3️⃣ MOSTRAR PAPELETA
        public async Task<IActionResult> Papeleta()
        {
            var cedula = HttpContext.Session.GetString("Cedula");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            if (cedula == null || pid == null) return RedirectToAction("Index");

            var candidatos = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{pid}");
            return View(candidatos.Data);
        }

        // 4️⃣ REGISTRAR EL VOTO
        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId)
        {
            // Recuperamos datos de sesión
            var cedula = HttpContext.Session.GetString("Cedula");
            var codigo = HttpContext.Session.GetString("Codigo");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            // Si se perdió la sesión (doble clic), volvemos al inicio suavemente
            if (cedula == null || codigo == null) return RedirectToAction("Index");

            string url = $"Votos/registrar-voto?cedula={cedula}&codigo={codigo}&candidatoId={candidatoId}&procesoElectoralId={pid}";

            var resultado = await _apiService.PostAsync<object>(url, null);

            if (resultado.Success)
            {
                HttpContext.Session.Clear(); // Borramos sesión para evitar regresar
                return RedirectToAction("Exito");
            }
            else
            {
                ViewBag.Error = resultado.Message;
                // Recargamos candidatos para mostrar el error en la misma pantalla
                var cand = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{pid}");
                return View("Papeleta", cand.Data);
            }
        }

        public IActionResult Exito()
        {
            return View();
        }
    }
}