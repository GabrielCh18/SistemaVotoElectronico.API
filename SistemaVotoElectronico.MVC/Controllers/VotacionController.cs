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

        // 1. PANTALLA DE INGRESO (LOGIN)
        public IActionResult Index()
        {
            return View();
        }

        // 2. VALIDAR CREDENCIALES
        [HttpPost]
        public async Task<IActionResult> Ingresar(string cedula, string codigo)
        {
            // Preguntamos a la API
            var respuesta = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

            // CASO 1: FALLÓ LA CONEXIÓN O LA API DIO ERROR
            if (!respuesta.Success)
            {
                // Aquí veremos si es culpa del SSL, del Puerto o de la API
                ViewBag.Error = $"🔥 ERROR DE CONEXIÓN: {respuesta.Message}";
                return View("Index");
            }

            // CASO 2: CONECTÓ PERO NO TRAJO DATOS
            if (respuesta.Data == null)
            {
                ViewBag.Error = "❌ La API respondió bien, pero el votante venía vacío (null).";
                return View("Index");
            }

            // CASO 3: YA VOTÓ
            if (respuesta.Data.YaVoto)
            {
                ViewBag.Error = "⚠️ Este ciudadano ya ejerció su voto.";
                return View("Index");
            }

            // SI LLEGAMOS AQUÍ, TODO ESTÁ PERFECTO
            TempData["Cedula"] = cedula;
            TempData["Codigo"] = codigo;
            return RedirectToAction("Papeleta");
        }

        // 3. PANTALLA DE PAPELETA (CANDIDATOS)
        public async Task<IActionResult> Papeleta()
        {
            // Recuperamos los datos (si se pierden, volver al login)
            if (TempData["Cedula"] == null) return RedirectToAction("Index");
            TempData.Keep(); // Mantener los datos para el siguiente paso (Votar)

            // Traemos los candidatos de la elección #1
            var respuesta = await _apiService.GetListAsync<Candidato>("Candidatos/por-proceso/1");
            return View(respuesta.Data);
        }

        // 4. ACCIÓN DE VOTAR
        [HttpPost]
        public async Task<IActionResult> Votar(int candidatoId)
        {
            var cedula = TempData["Cedula"]?.ToString();
            var codigo = TempData["Codigo"]?.ToString();
            int procesoId = 1; // Elección 2026

            // Enviamos el voto a la API
            // Nota: Enviamos los datos en la URL porque así lo espera tu controlador actual
            string url = $"Votos/registrar-voto?cedula={cedula}&codigo={codigo}&candidatoId={candidatoId}&procesoElectoralId={procesoId}";

            var respuesta = await _apiService.PostAsync<object>(url, null);

            if (respuesta.Success)
            {
                return RedirectToAction("Exito");
            }
            else
            {
                ViewBag.Error = "❌ Error al votar: " + respuesta.Message;
                // Recargamos los candidatos para que intente de nuevo
                var candidatos = await _apiService.GetListAsync<Candidato>("Candidatos/por-proceso/1");
                return View("Papeleta", candidatos.Data);
            }
        }

        public IActionResult Exito()
        {
            return View();
        }
    }
}