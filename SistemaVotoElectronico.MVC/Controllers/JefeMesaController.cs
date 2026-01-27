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
        // 1. LOGIN DEL JEFE (LA PUERTA DE ENTRADA)
        // ==========================================
        public IActionResult Login()
        {
            // Si ya está adentro, lo mandamos directo al trabajo
            if (HttpContext.Session.GetString("JefeLogueado") != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "⚠️ Debes ingresar tu número de cédula.";
                return View();
            }

            // 1. Buscamos al ciudadano en la base de datos
            var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

            if (!response.Success || response.Data == null)
            {
                ViewBag.Error = "❌ No encontramos esa cédula en el sistema.";
                return View();
            }

            var jefe = response.Data;

            // 2. VERIFICAMOS LA INSIGNIA (El check que creamos antes)
            if (!jefe.EsJefe)
            {
                ViewBag.Error = "⛔ Lo sentimos, usted NO está registrado como Jefe de Junta.";
                return View();
            }

            // 3. ¡ÉXITO! Guardamos sus datos en sesión para que el sistema lo recuerde
            HttpContext.Session.SetString("JefeLogueado", jefe.Nombre + " " + jefe.Apellido);
            HttpContext.Session.SetInt32("JuntaIdJefe", jefe.JuntaId); // Guardamos qué mesa cuida

            return RedirectToAction("Index");
        }

        public IActionResult Salir()
        {
            HttpContext.Session.Clear(); // Borramos la sesión
            return RedirectToAction("Login");
        }

        // ==========================================
        // 2. EL PANEL DE CONTROL (PROTEGIDO)
        // ==========================================
        public IActionResult Index()
        {
            // SEGURIDAD: Si no se ha logueado, para afuera
            if (HttpContext.Session.GetString("JefeLogueado") == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
            return View();
        }

        // ==========================================
        // 3. GENERAR CÓDIGO (ACCIÓN PRINCIPAL)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Generar(string cedulaVotante)
        {
            // Validamos que siga logueado
            if (HttpContext.Session.GetString("JefeLogueado") == null) return RedirectToAction("Login");

            int? juntaDelJefe = HttpContext.Session.GetInt32("JuntaIdJefe");

            if (string.IsNullOrEmpty(cedulaVotante))
            {
                ViewBag.Error = "⚠️ Ingrese la cédula del votante.";
                ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
                return View("Index");
            }

            // 1. Buscamos al votante que quiere votar
            var responseVotante = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedulaVotante}");

            if (!responseVotante.Success)
            {
                ViewBag.Error = "❌ Votante no encontrado.";
                ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
                return View("Index");
            }

            var votante = responseVotante.Data;

            // 2. REGLA DE ORO: ¿El votante pertenece a MI mesa?
            if (votante.JuntaId != juntaDelJefe)
            {
                ViewBag.Error = $"⛔ ALERTA: Este votante NO pertenece a su mesa. Él debe votar en la Mesa {votante.Junta?.Numero} del recinto {votante.Junta?.Zona?.Nombre}.";
                ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
                return View("Index");
            }

            // 3. Si todo está bien, generamos el código
            var response = await _apiService.PostWithResponseAsync<RespuestaCodigo, RespuestaCodigo>(
                $"Votantes/generar-codigo/{cedulaVotante}",
                new RespuestaCodigo()
            );

            if (response.Success && response.Data != null)
            {
                ViewBag.CodigoGenerado = response.Data.Codigo;
                ViewBag.NombreVotante = response.Data.Nombre;
                ViewBag.Mensaje = "✅ CÓDIGO GENERADO";
            }
            else
            {
                ViewBag.Error = "❌ " + response.Message;
            }

            ViewBag.JefeNombre = HttpContext.Session.GetString("JefeLogueado");
            return View("Index");
        }

        public class RespuestaCodigo
        {
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}