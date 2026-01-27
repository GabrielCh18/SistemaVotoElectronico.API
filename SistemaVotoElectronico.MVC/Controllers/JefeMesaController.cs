using Microsoft.AspNetCore.Mvc;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class JefeMesaController : Controller
    {
        private readonly ApiService _apiService;

        public JefeMesaController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ==========================================
        // LOGIN (Igual que antes)
        // ==========================================
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("JefeLogueado") != null) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string cedula)
        {
            if (string.IsNullOrEmpty(cedula)) { ViewBag.Error = "⚠️ Ingresa la cédula."; return View(); }

            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");
            if (!response.Success || response.Data == null) { ViewBag.Error = "❌ Cédula no encontrada."; return View(); }

            var jefe = response.Data;
            if (!jefe.EsJefe) { ViewBag.Error = "⛔ No tienes permisos de Jefe de Junta."; return View(); }

            HttpContext.Session.SetString("JefeLogueado", $"{jefe.Nombre} {jefe.Apellido}");
            HttpContext.Session.SetInt32("JuntaIdJefe", jefe.JuntaId);

            return RedirectToAction("Index");
        }

        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ==========================================
        // 2. PANEL DE CONTROL (AHORA CON BÚSQUEDA)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(string? cedulaBusqueda)
        {
            if (HttpContext.Session.GetString("JefeLogueado") == null) return RedirectToAction("Login");

            ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
            int? juntaDelJefe = HttpContext.Session.GetInt32("JuntaIdJefe");

            // Si no hay búsqueda, mostramos la pantalla limpia
            if (string.IsNullOrEmpty(cedulaBusqueda)) return View();

            // BUSCAMOS AL VOTANTE
            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedulaBusqueda}");

            if (response.Success && response.Data != null)
            {
                var votante = response.Data;

                // Pasamos datos clave a la vista para decidir qué color mostrar
                ViewBag.EsDeMiMesa = (votante.JuntaId == juntaDelJefe);
                ViewBag.MesaCorrecta = votante.Junta?.Numero; // Para decirle a cuál ir si se equivocó
                ViewBag.RecintoCorrecto = votante.Junta?.Zona?.Nombre;

                return View(votante); // Enviamos el Votante encontrado a la vista
            }
            else
            {
                ViewBag.Error = "❌ No se encontró ningún ciudadano con esa cédula.";
                return View();
            }
        }

        // ==========================================
        // 3. GENERAR CÓDIGO (SOLO SI EL JEFE CONFIRMA)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Generar(string cedulaVotante)
        {
            if (HttpContext.Session.GetString("JefeLogueado") == null) return RedirectToAction("Login");

            // Llamamos a la API para generar (Tu API ya valida si ya votó, etc.)
            var response = await _apiService.PostWithResponseAsync<RespuestaCodigo, RespuestaCodigo>(
                $"Votantes/generar-codigo/{cedulaVotante}",
                new RespuestaCodigo()
            );

            if (response.Success && response.Data != null)
            {
                // ÉXITO: Mostramos el código
                TempData["CodigoGenerado"] = response.Data.Codigo;
                TempData["NombreVotante"] = response.Data.Nombre;
                return RedirectToAction("Index"); // Recargamos para limpiar
            }
            else
            {
                // ERROR: Lo mostramos
                ViewBag.ErrorGenerar = response.Message;
                ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
                return View("Index");
            }
        }
    }

    // Tu clase auxiliar
    public class RespuestaCodigo
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}