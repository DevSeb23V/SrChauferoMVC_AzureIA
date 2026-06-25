using Microsoft.AspNetCore.Mvc;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLogged()
        {
            return HttpContext.Session.GetString("Usuario") != null;
        }

        protected bool IsAdmin()
        {
            return HttpContext.Session.GetString("Rol") == "Administrador";
        }

        protected bool IsUsuario()
        {
            return HttpContext.Session.GetString("Rol") == "Usuario";
        }

        protected IActionResult RequireLogin()
        {
            if (!IsLogged())
            {
                return RedirectToAction("Login", "Account");
            }

            return new EmptyResult();
        }

        protected IActionResult RequireAdmin()
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Platos");
            }

            return new EmptyResult();
        }
    }
}
