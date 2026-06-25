using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class HomeController : BaseController
    {
        // ==========================================
        // INYECCIÓN DEL CONTEXTO DE BASE DE DATOS
        // ==========================================
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // DASHBOARD PRINCIPAL
        // ==========================================
        public IActionResult Index()
        {
            // Validar sesión
            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (HttpContext.Session.GetString("Rol") == "Usuario")
            {
                return RedirectToAction("Index", "Platos");
            }

            // Estadísticas generales
            ViewBag.Platos = _db.Platos.Count();
            ViewBag.Pedidos = _db.Pedidos.Count();
            ViewBag.Mesas = _db.Mesas.Count();

            // Estado de mesas
            ViewBag.MesasOcupadas = _db.Mesas.Count(x => x.Estado == "Ocupada");
            ViewBag.MesasLibres = _db.Mesas.Count(x => x.Estado == "Libre");

            // Ventas
            ViewBag.VentasTotales = _db.Pedidos.Sum(x => x.Total);

            ViewBag.VentasDia = _db.Pedidos
                .Where(x => x.Fecha.Date == DateTime.Today)
                .Sum(x => x.Total);

            return View();
        }
    }
}