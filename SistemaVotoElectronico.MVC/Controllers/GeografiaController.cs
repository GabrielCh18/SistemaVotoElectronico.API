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

        [HttpGet]
        public async Task<JsonResult> ObtenerParroquias(int cantonId)
        {
            var respuesta = await _apiService.GetListAsync<Parroquia>($"Geografia/parroquias/por-canton/{cantonId}");
            return Json(respuesta.Data);
        }

        // B. La Vista de Crear Zona
        public async Task<IActionResult> CrearZona()
        {
            // Carga inicial: Solo las Provincias (Nivel 1)
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearZona(Zona zona)
        {
            // Ignoramos validaciones de navegación
            ModelState.Remove("Parroquia");
            ModelState.Remove("Juntas");

            if (!ModelState.IsValid)
            {
                // Si falla, recargamos solo el nivel 1 (Provincias)
                // El usuario tendrá que seleccionar de nuevo (es difícil mantener el estado de los 3 niveles tras un error)
                var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
                ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
                return View(zona);
            }

            var response = await _apiService.PostAsync("Geografia/zonas", zona);

            if (response.Success) return RedirectToAction("Index");

            // Manejo de error de API
            ViewBag.Error = response.Message;
            var provs = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provs.Data, "Id", "Nombre");

            return View(zona);
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerZonas(int parroquiaId)
        {
            var respuesta = await _apiService.GetListAsync<Zona>($"Geografia/zonas/por-parroquia/{parroquiaId}");
            return Json(respuesta.Data);
        }

        // B. Pantalla Crear Junta
        public async Task<IActionResult> CrearJunta()
        {
            // Carga inicial: Solo nivel 1 (Provincias)
            var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearJunta(Junta junta)
        {
            // Ignoramos la validación del objeto padre (Zona)
            ModelState.Remove("Zona");

            if (!ModelState.IsValid)
            {
                // Si falla, recargamos Provincias
                var provincias = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
                ViewBag.Provincias = new SelectList(provincias.Data, "Id", "Nombre");
                return View(junta);
            }

            var response = await _apiService.PostAsync("Geografia/juntas", junta);

            if (response.Success) return RedirectToAction("Index");

            // Error
            ViewBag.Error = response.Message;
            var provs = await _apiService.GetListAsync<Provincia>("Geografia/provincias");
            ViewBag.Provincias = new SelectList(provs.Data, "Id", "Nombre");
            return View(junta);
        }
        [HttpGet]
        public async Task<JsonResult> ObtenerJuntas(int zonaId)
        {
            var respuesta = await _apiService.GetListAsync<Junta>($"Geografia/juntas/por-zona/{zonaId}");

            // Transformamos la data para que muestre "Mesa 1 - Masculino" en el combo
            var lista = respuesta.Data?.Select(j => new {
                id = j.Id,
                nombre = $"Mesa {j.Numero} - {j.Genero}"
            });

            return Json(lista);
        }
    }
}