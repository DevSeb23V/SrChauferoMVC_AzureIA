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

            if (vm.Usuario == "admin" && vm.Password == "Admin123")
            {
                CrearSesion("admin", "Administrador", "Administrador");
                return RedirectToAction("Index", "Home");
            }

            if (vm.Usuario == "cocinero" && vm.Password == "Cocinero123")
            {
                CrearSesion("cocinero", "Cocinero", "Cocinero");
                return RedirectToAction("Index", "Cocina");
            }

            if (vm.Usuario == "mozo" && vm.Password == "Mozo123")
            {
                CrearSesion("mozo", "Mozo", "Mozo");
                return RedirectToAction("Index", "Mozo");
            }

            if (vm.Usuario == "cliente" && vm.Password == "Cliente123")
            {
                CrearSesion("cliente", "Cliente", "Cliente");
                return RedirectToAction("Index", "Cliente");
            }

            vm.Error = "Usuario y/o contraseña incorrectos.";
            GenerateCaptcha();
            return View(vm);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private void CrearSesion(string usuario, string nombre, string rol)
        {
            HttpContext.Session.SetString("Usuario", usuario);
            HttpContext.Session.SetString("Nombre", nombre);
            HttpContext.Session.SetString("Rol", rol);
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