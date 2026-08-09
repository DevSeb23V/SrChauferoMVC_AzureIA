using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Models;
using SrChauferoMVC_AzureIA.Services;
using SrChauferoMVC_AzureIA.ViewModels;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService;


        public AccountController(
            ApplicationDbContext db,
            EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }



        // ===============================
        // LOGIN GET
        // ===============================

        [HttpGet]
        public IActionResult Login()
        {
            GenerateCaptcha();

            return View(new LoginViewModel());
        }



        // ===============================
        // LOGIN POST
        // ===============================

        [HttpPost]
        public IActionResult Login(LoginViewModel vm)
        {

            int correct =
                HttpContext.Session.GetInt32("Captcha") ?? -1;



            if (vm.CaptchaRespuesta != correct)
            {
                vm.Error =
                    "Captcha incorrecto.";

                GenerateCaptcha();

                return View(vm);
            }



            var usuario = _db.Usuarios
                .Include(x => x.Rol)
                .FirstOrDefault(x =>
                    (x.NombreUsuario == vm.Usuario ||
                     x.Correo == vm.Usuario)
                    &&
                    x.Password == vm.Password
                    &&
                    x.Activo
                );



            if (usuario == null)
            {

                vm.Error =
                    "Usuario o contraseña incorrectos.";


                GenerateCaptcha();


                return View(vm);
            }



            EnviarCodigo(usuario);



            return RedirectToAction(
                "VerificarCodigo"
            );

        }





        // ===============================
        // ENVIO CODIGO 2FA
        // ===============================


        private void EnviarCodigo(
            Usuario usuario)
        {


            Random rnd = new Random();


            string codigo =
                rnd.Next(100000, 999999)
                .ToString();



            HttpContext.Session.SetString(
                "Codigo2FA",
                codigo);



            HttpContext.Session.SetString(
                 "UsuarioPendiente",
                 usuario.NombreUsuario);

            HttpContext.Session.SetString(
                "NombrePendiente",
                usuario.Nombre);

            HttpContext.Session.SetString(
                "RolPendiente",
                usuario.Rol?.Nombre ?? "");

            HttpContext.Session.SetString(
                "CorreoPendiente",
                usuario.Correo);



            _emailService.EnviarCodigo(
                usuario.Correo,
                codigo
            );


        }





        // ===============================
        // VERIFICAR CODIGO GET
        // ===============================


        [HttpGet]
        public IActionResult VerificarCodigo()
        {


            string usuario =
                HttpContext.Session.GetString(
                    "UsuarioPendiente"
                );



            if (usuario == null)
            {
                return RedirectToAction(
                    "Login"
                );
            }



            string correo =
                HttpContext.Session.GetString(
                    "CorreoPendiente"
                );



            ViewBag.Correo =
                OcultarCorreo(correo);



            return View();

        }







        // ===============================
        // VERIFICAR CODIGO POST
        // ===============================


        [HttpPost]
        public IActionResult VerificarCodigo(
            string codigo)
        {


            string codigoGuardado =
                HttpContext.Session.GetString(
                    "Codigo2FA"
                );



            if (codigo == codigoGuardado)
            {


                CrearSesion(
                    HttpContext.Session.GetString(
                        "UsuarioPendiente"
                    ),

                    HttpContext.Session.GetString(
                        "NombrePendiente"
                    ),

                    HttpContext.Session.GetString(
                        "RolPendiente"
                    )
                );



                HttpContext.Session.Remove(
                    "Codigo2FA"
                );



                return RedireccionarPorRol();

            }



            ViewBag.Error =
                "Código incorrecto.";


            return View();

        }







        // ===============================
        // OCULTAR CORREO
        // ===============================


        private string OcultarCorreo(
            string correo)
        {


            if (string.IsNullOrEmpty(correo))
                return "";



            var partes =
                correo.Split("@");



            string usuario =
                partes[0];


            string dominio =
                partes[1];



            if (usuario.Length <= 3)
                return correo;



            return usuario.Substring(0, 3)
                + "****@"
                + dominio;

        }







        // ===============================
        // CREAR SESION
        // ===============================


        private void CrearSesion(
            string usuario,
            string nombre,
            string rol)
        {

            HttpContext.Session.SetString(
                "Usuario",
                usuario);



            HttpContext.Session.SetString(
                "Nombre",
                nombre);



            HttpContext.Session.SetString(
                "Rol",
                rol);

        }







        // ===============================
        // REDIRECCION POR ROL
        // ===============================


        private IActionResult RedireccionarPorRol()
        {

            string rol =
                HttpContext.Session.GetString(
                    "Rol"
                );



            switch (rol)
            {


                case "Administrador":

                    return RedirectToAction(
                        "Index",
                        "Home"
                    );



                case "Cocinero":

                    return RedirectToAction(
                        "Index",
                        "Cocina"
                    );



                case "Mozo":

                    return RedirectToAction(
                        "Index",
                        "Mozo"
                    );



                case "Cliente":

                    return RedirectToAction(
                        "Index",
                        "Cliente"
                    );



                default:

                    return RedirectToAction(
                        "Login"
                    );

            }

        }







        // ===============================
        // LOGOUT
        // ===============================


        public IActionResult Logout()
        {

            HttpContext.Session.Clear();


            return RedirectToAction(
                "Login"
            );

        }







        // ===============================
        // CAPTCHA
        // ===============================


        private void GenerateCaptcha()
        {

            Random rnd =
                new Random();



            int a =
                rnd.Next(1, 9);



            int b =
                rnd.Next(1, 9);



            HttpContext.Session.SetInt32(
                "Captcha",
                a + b
            );



            ViewBag.Captcha =
                $"{a} + {b}";

        }

    }
}