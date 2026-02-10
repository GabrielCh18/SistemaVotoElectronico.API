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

        public IActionResult Index()
        {
            HttpContext.Session.Remove("Cedula");
            HttpContext.Session.Remove("Codigo");
            HttpContext.Session.Remove("ProcesoId");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ingresar(string cedula, string codigo)
        {
            if (string.IsNullOrEmpty(cedula) || string.IsNullOrEmpty(codigo))
            {
                ViewBag.Error = "⚠️ Ingresa cédula y código.";
                return View("Index");
            }

            var proceso = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");
            if (!proceso.Success || proceso.Data == null)
            {
                ViewBag.Error = "❌ No hay votaciones activas.";
                return View("Index");
            }

            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");
            if (!response.Success || response.Data == null)
            {
                ViewBag.Error = "❌ Cédula no encontrada.";
                return View("Index");
            }

            var votante = response.Data;
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

            HttpContext.Session.SetString("Cedula", cedula);
            HttpContext.Session.SetString("Codigo", codigo);
            HttpContext.Session.SetInt32("ProcesoId", proceso.Data.Id);

            // EMPEZAMOS POR ASAMBLEISTAS
            return RedirectToAction("Papeleta", new { fase = "Asambleistas" });
        }

        public async Task<IActionResult> Papeleta(string fase)
        {
            var cedula = HttpContext.Session.GetString("Cedula");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            if (cedula == null || pid == null) return RedirectToAction("Index");

            var respuesta = await _apiService.GetListAsync<Candidato>($"Candidatos/por-proceso/{pid}");

            if (!respuesta.Success || respuesta.Data == null)
            {
                ViewBag.Error = "⚠️ Error cargando los datos.";
                return View("Index");
            }

            List<Candidato> filtrados;
            if (fase == "Asambleistas")
            {
                filtrados = respuesta.Data.Where(c => c.Dignidad.Contains("Asambleísta")).ToList();
                ViewBag.Paso = "Asambleístas";
                ViewBag.Icono = "🏛️";
            }
            else
            {
                filtrados = respuesta.Data.Where(c => c.Dignidad == "Presidente").ToList();
                ViewBag.Paso = "Presidente";
                ViewBag.Icono = "🗳️";
            }

            ViewBag.FaseActual = fase;
            return View(filtrados);
        }

        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId, string faseActual)
        {
            var cedula = HttpContext.Session.GetString("Cedula");
            var codigo = HttpContext.Session.GetString("Codigo");
            var pid = HttpContext.Session.GetInt32("ProcesoId");

            if (cedula == null || codigo == null || pid == null) return RedirectToAction("Index");

            // Si votó por asambleísta, simplemente pasamos a la siguiente fase sin registrar en BD aún 
            // (O puedes registrarlo si tu BD soporta múltiples votos por persona)
            if (faseActual == "Asambleistas")
            {
                return RedirectToAction("Papeleta", new { fase = "Presidente" });
            }

            // Si es la fase final (Presidente), registramos el voto oficialmente
            string url = $"Votos/registrar-voto?cedula={cedula}&codigo={codigo}&candidatoId={candidatoId}&procesoElectoralId={pid}";
            var resultado = await _apiService.PostAsync<object>(url, null);

            if (resultado.Success)
            {
                HttpContext.Session.Remove("Cedula");
                HttpContext.Session.Remove("Codigo");
                HttpContext.Session.Remove("ProcesoId");
                return RedirectToAction("Exito");
            }
            else
            {
                ViewBag.Error = resultado.Message;
                return RedirectToAction("Papeleta", new { fase = "Presidente" });
            }
        }

        public IActionResult Exito() => View();
    }
}