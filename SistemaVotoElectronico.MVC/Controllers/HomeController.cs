using Microsoft.AspNetCore.Mvc;
using SistemaVotoElectronico.MVC.Models;
using SistemaVotoElectronico.ApiConsumer;
using SistemaVoto.Modelos;
using System.Diagnostics;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _apiService;

        public HomeController(ILogger<HomeController> logger, ApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        // 👇 ESTE ES EL MÉTODO ÚNICO (FUSIONADO)
        public async Task<IActionResult> Index(string? cedula)
        {
            // ---------------------------------------------------------
            // PARTE 1: VERIFICAR ESTADO DE ELECCIÓN (Para el botón)
            // ---------------------------------------------------------
            var procesoActivo = await _apiService.GetAsync<ProcesoElectoral>("ProcesosElectorales/activo");

            // Si hay un proceso activo (datos existen y no es nulo), estamos votando
            bool hayEleccionEnCurso = (procesoActivo.Success && procesoActivo.Data != null);

            // LOGICA CORREGIDA DEL BOTÓN:
            // Solo mostramos resultados si NO hay elección en curso Y hay historial previo
            if (hayEleccionEnCurso)
            {
                ViewBag.MostrarResultados = false; // Ocultar botón
                ViewBag.MensajeEstado = "🗳️ Elecciones en Curso"; // Mensaje informativo
            }
            else
            {
                // Si no hay activa, miramos si hubo alguna antes para habilitar el botón
                var historial = await _apiService.GetListAsync<ProcesoElectoral>("ProcesosElectorales");
                bool hayHistorial = historial.Success && historial.Data != null && historial.Data.Any();

                ViewBag.MostrarResultados = hayHistorial; // Mostrar botón si hay algo que mostrar
            }

            // ---------------------------------------------------------
            // PARTE 2: LÓGICA DEL BUSCADOR (Si escribieron cédula)
            // ---------------------------------------------------------
            if (!string.IsNullOrEmpty(cedula))
            {
                var response = await _apiService.GetAsync<Votante>($"Votantes/buscar/{cedula}");

                if (response.Success && response.Data != null)
                {
                    return View(response.Data); // Retorna vista CON el votante encontrado
                }
                else
                {
                    ViewBag.Error = "⚠️ No encontramos esa cédula en el padrón electoral.";
                    ViewBag.CedulaBuscada = cedula;
                }
            }

            // Si no buscaron nada, o no encontraron, retornamos la vista normal
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost] // Usamos POST para seguridad
        public async Task<IActionResult> ObtenerCertificado(int id, string nombre, string cedula, string proceso)
        {
            // 1. Marcamos en la BD que ya se descargó
            await _apiService.PostAsync<object>($"Votantes/marcar-certificado/{id}", null);

            // 2. Pasamos los datos a la vista del diploma
            ViewBag.NombreCompleto = nombre;
            ViewBag.Cedula = cedula;
            ViewBag.Proceso = proceso;
            ViewBag.Fecha = DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy");

            return View("Certificado");
        }
    }
}