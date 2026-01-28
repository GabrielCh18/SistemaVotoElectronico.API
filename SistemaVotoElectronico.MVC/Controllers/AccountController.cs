using Microsoft.AspNetCore.Mvc;

namespace SistemaVotoElectronico.MVC.Controllers
{
    public class AccountController : Controller
    {
        // PANTALLA DE LOGIN
        public IActionResult Login()
        {
            return View();
        }

        // PROCESAR LOGIN
        [HttpPost]
        public IActionResult Login(string usuario, string password)
        {
            // Validamos usuario (Gabriel o Andres) y contraseña (Secr3ta55)
            if ((usuario == "Gabriel" || usuario == "Andres") && password == "Secr3ta55")
            {
                // ¡ÉXITO! Guardamos el nombre en la sesión
                HttpContext.Session.SetString("UsuarioAdmin", usuario);
                return RedirectToAction("Candidatos", "Admin");
            }

            // FALLÓ
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        // SALIR
        public IActionResult Logout()
        {
            // 1. Borramos la sesión
            HttpContext.Session.Clear();

            // 2. Redirigimos AL MISMO LOGIN (En vez de al Home)
            return RedirectToAction("Login");
        }
    }
}