using Microsoft.AspNetCore.Mvc;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLogged()
        {
            return HttpContext.Session.GetString("Usuario") != null;
        }

        protected string RolActual()
        {
            return HttpContext.Session.GetString("Rol") ?? "";
        }

        protected bool IsAdmin()
        {
            return RolActual() == "Administrador";
        }

        protected bool IsCocinero()
        {
            return RolActual() == "Cocinero";
        }

        protected bool IsMozo()
        {
            return RolActual() == "Mozo";
        }

        protected bool IsCliente()
        {
            return RolActual() == "Cliente";
        }

        protected IActionResult RequireLogin()
        {
            if (!IsLogged())
            {
                return RedirectToAction("Login", "Account");
            }

            return new EmptyResult();
        }

        protected IActionResult RequireRole(params string[] roles)
        {

            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (!roles.Contains(RolActual()))
            {
                return RedirectToAction("Login", "Account");
            }

            return new EmptyResult();
        }

        protected IActionResult RequireAdmin()
        {
            return RequireRole("Administrador");
        }

        protected IActionResult RequireCocinero()
        {
            return RequireRole("Cocinero", "Administrador");
        }

        protected IActionResult RequireMozo()
        {
            return RequireRole("Mozo", "Administrador");
        }

        protected IActionResult RequireCliente()
        {
            return RequireRole("Cliente", "Administrador");
        }
        protected IActionResult RequireIA()
        {
            return RequireRole(
                "Administrador",
                "Cliente",
                "Mozo",
                "Cocinero"
            );
        }
    }
}