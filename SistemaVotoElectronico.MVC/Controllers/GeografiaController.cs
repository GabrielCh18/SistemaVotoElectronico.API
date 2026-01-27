using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVoto.Modelos;
using SistemaVotoElectronico.ApiConsumer;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class GeografiaController : Controller
    {
        private readonly ApiService _apiService;

        public GeografiaController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // VISTA PRINCIPAL (MENU)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UsuarioAdmin") == null)
                return RedirectToAction("Login", "Account");

            // Pedimos las provincias a la API para mostrarlas en la tabla
            var respuesta = await _apiService.GetListAsync<Provincia>("Geografia/provincias");

            // Si hay datos los mandamos, si no, una lista vacía
            return View(respuesta.Success ? respuesta.Data : new List<Provincia>());
        }

        // 2. AGREGAMOS LA ACCIÓN DE ELIMINAR
        public async Task<IActionResult> EliminarProvincia(int id)
        {
            var respuesta = await _apiService.DeleteAsync($"Geografia/provincias/{id}");

            if (!respuesta.Success)
            {
                TempData["Error"] = "No se pudo eliminar: " + respuesta.Message;
            }

            return RedirectToAction("Index");
        }

        // ------------------------------------------------
        // 1. CREAR PROVINCIA
        // ------------------------------------------------
        public IActionResult CrearProvincia() => View();

        [HttpPost]
        public async Task<IActionResult> CrearProvincia(Provincia provincia)
        {
            ModelState.Remove("Cantones");

            if (!ModelState.IsValid) return View(provincia);

            var response = await _apiService.PostAsync("Geografia/provincias", provincia);
            if (response.Success) return RedirectToAction("Index");

            ViewBag.Error = response.Message;
            return View(provincia);
        }

        // ------------------------------------------------
        // 2. CREAR CANTON (Necesita cargar Provincias)
        // ------------------------------------------------
        public async Task<IActionResult> CrearCanton()
        {
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCanton(Canton canton)
        {
            ModelState.Remove("Provincia");
            ModelState.Remove("Parroquias");

            var response = await _apiService.PostAsync("Geografia/cantones", canton);
            if (response.Success) return RedirectToAction("Index");

            // Recargar lista si falla
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
            ViewBag.Error = response.Message;
            return View(canton);
        }

        public async Task<IActionResult> CrearParroquia()
        {
            // CAMBIO: Ahora cargamos PROVINCIAS, no Cantones directamente
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");

            return View();
        }
        [HttpGet]
        public async Task<JsonResult> ObtenerCantones(int provinciaId)
        {
            // Llamamos a la API que ya creamos antes: "api/Geografia/cantones/por-provincia/{id}"
            var respuesta = await _apiService.GetListAsync<Canton>($"Geografia/cantones/por-provincia/{provinciaId}");

            // Devolvemos los datos en formato JSON para que JavaScript los entienda
            return Json(respuesta.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CrearParroquia(Parroquia parroquia)
        {
            ModelState.Remove("Canton");
            ModelState.Remove("Zonas");

            if (!ModelState.IsValid)
            {
                // Si falla, recargamos las PROVINCIAS (para que no se rompa la vista)
                var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
                ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
                return View(parroquia);
            }

            var response = await _apiService.PostAsync("Geografia/parroquias", parroquia);

            if (response.Success) return RedirectToAction("Index");

            ViewBag.Error = response.Message;
            var provs = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provs.Data, "Id", "Nombre");

            return View(parroquia);
        }

        // ------------------------------------------------
        // 3. CREAR JUNTA (Ejemplo final de la cadena)
        // Para simplificar, aquí asumimos que sabes el ID de la Zona o
        // podrías hacer una carga en cascada con JavaScript, pero para 
        // empezar simple, cargaremos TODAS las zonas.
        // ------------------------------------------------
        public async Task<IActionResult> CrearJunta()
        {
            // Nota: En un sistema real harías combos en cascada (Prov->Canton->Parr->Zona)
            // Aquí cargaremos solo Zonas para simplificar el código inicial.
            // Necesitarías crear un endpoint "Geografia/zonas" que traiga todas si quieres listarlas todas,
            // o buscar primero la parroquia. 
            // POR AHORA: Asumiremos que creas una API que traiga todas las zonas o usas IDs directos.
            // Para no complicarte con JavaScript ahora mismo, lo dejaré simple:

            return View();
        }

        // NOTA: Para las Parroquias y Zonas la lógica es idéntica a Cantón,
        // solo cambia qué lista cargas (Cantones para Parroquias, Parroquias para Zonas).
    }
}