using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.ViewModels;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            GenerateCaptcha();
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel vm)
        {
            int correct = HttpContext.Session.GetInt32("Captcha") ?? -1;

            if (vm.CaptchaRespuesta != correct)
            {
                vm.Error = "Captcha incorrecto. Inténtalo nuevamente.";
                GenerateCaptcha();
                return View(vm);
            }

            // Perfil administrador de demostración
            if (vm.Usuario == "admin" && vm.Password == "Admin123")
            {
                HttpContext.Session.SetString("Usuario", "admin");
                HttpContext.Session.SetString("Nombre", "Administrador");
                HttpContext.Session.SetString("Rol", "Administrador");
                return RedirectToAction("Index", "Home");
            }

            // Perfil usuario de demostración
            if (vm.Usuario == "usuario" && vm.Password == "Usuario123")
            {
                HttpContext.Session.SetString("Usuario", "usuario");
                HttpContext.Session.SetString("Nombre", "Usuario Cliente");
                HttpContext.Session.SetString("Rol", "Usuario");
                return RedirectToAction("Index", "Mesas");
            }

            vm.Error = "Usuario y/o contraseña incorrectos.";
            GenerateCaptcha();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.Password != vm.ConfirmarPassword)
            {
                vm.Error = "Las contraseñas no coinciden.";
                return View(vm);
            }

            // Registro simple para el perfil de usuario/cliente.
            // Para el proyecto académico se crea la sesión directamente como rol Usuario.
            HttpContext.Session.SetString("Usuario", vm.Usuario.Trim());
            HttpContext.Session.SetString("Nombre", vm.NombreCompleto.Trim());
            HttpContext.Session.SetString("Rol", "Usuario");

            return RedirectToAction("Index", "Mesas");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int a = rnd.Next(1, 9);
            int b = rnd.Next(1, 9);
            HttpContext.Session.SetInt32("Captcha", a + b);
            ViewBag.Captcha = $"{a} + {b}";
        }
    }
}
