using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class VotantesController : Controller
    {
        private readonly ApiService _apiService;

        public VotantesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // 🔒 CANDADO DE SEGURIDAD
        private bool NoEsAdmin() => HttpContext.Session.GetString("UsuarioAdmin") == null;

        // 1. CREAR VOTANTE (Ahora protegido)
        public async Task<IActionResult> Crear()
        {
            // SI NO ES ADMIN -> LO MANDAMOS AL LOGIN
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // Cargamos el nivel 1 (Provincias)
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Votante votante)
        {
            // SI NO ES ADMIN -> LO MANDAMOS AL LOGIN
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            ModelState.Remove("Junta");

            if (!ModelState.IsValid)
            {
                var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
                ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
                return View(votante);
            }

            var response = await _apiService.PostAsync("Votantes/registrar", votante);

            if (response.Success)
            {
                TempData["Mensaje"] = "✅ ¡Ciudadano registrado correctamente!";
                return RedirectToAction("Crear");
            }

            ViewBag.Error = response.Message;
            var provs = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provs.Data, "Id", "Nombre");

            return View(votante);
        }
        // --------------------------------------------------
        // 2. LISTADO GENERAL (PADRÓN)
        // --------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // Verificamos si es admin
            if (NoEsAdmin()) return RedirectToAction("Login", "Account");

            // Pedimos la lista completa al API
            var respuesta = await _apiService.GetListAsync<Votante>("Votantes");

            if (!respuesta.Success)
            {
                ViewBag.Error = "No se pudo cargar el padrón electoral.";
                return View(new List<Votante>());
            }

            return View(respuesta.Data);
        }
    }
}