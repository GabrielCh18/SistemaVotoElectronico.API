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

        //PANTALLA DE INGRESO
        public IActionResult Index()
        {
            // Limpieza selectiva para no sacar al Jefe
            HttpContext.Session.Remove("Cedula");
            HttpContext.Session.Remove("Codigo");
            HttpContext.Session.Remove("ProcesoId");
            return View();
        }

        //VALIDAR CÉDULA Y CÓDIGO
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

            // Validar Token
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

            // Guardar sesión
            HttpContext.Session.SetString("Cedula", cedula);
            HttpContext.Session.SetString("Codigo", codigo);
            HttpContext.Session.SetInt32("ProcesoId", proceso.Data.Id);

            return RedirectToAction("Papeleta");
        }

        // MOSTRAR PAPELETA
        public async Task<IActionResult> Papeleta()
        {
            var cedula = HttpContext.Session.GetString("Cedula");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            if (cedula == null || pid == null) return RedirectToAction("Index");

            var candidatos = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{pid}");

            // Si la API falla y devuelve null, no dejamos que la vista explote.
            if (!candidatos.Success || candidatos.Data == null)
            {
                ViewBag.Error = "⚠️ Error cargando la papeleta. Intente ingresar nuevamente.";
                return View("Index"); // Volvemos al login en vez de explotar
            }

            return View(candidatos.Data);
        }

        // REGISTRAR EL VOTO
        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId)
        {
            var cedula = HttpContext.Session.GetString("Cedula");
            var codigo = HttpContext.Session.GetString("Codigo");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            if (cedula == null || codigo == null || pid == null) return RedirectToAction("Index");

            string url = $"Votos/registrar-voto?cedula={cedula}&codigo={codigo}&candidatoId={candidatoId}&procesoElectoralId={pid}";

            var resultado = await _apiService.PostAsync<object>(url, null);

            if (resultado.Success)
            {
                // Limpieza selectiva (Mantiene al Jefe vivo)
                HttpContext.Session.Remove("Cedula");
                HttpContext.Session.Remove("Codigo");
                HttpContext.Session.Remove("ProcesoId");

                return RedirectToAction("Exito");
            }
            else
            {
                ViewBag.Error = resultado.Message;

                var cand = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{pid}");

                if (!cand.Success || cand.Data == null)
                {
                    ViewBag.Error = "❌ Error crítico: " + resultado.Message;
                    return View("Index"); 
                }

                return View("Papeleta", cand.Data);
            }
        }

        public IActionResult Exito()
        {
            return View();
        }
    }
}