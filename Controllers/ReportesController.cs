using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class ReportesController : BaseController
    {
        // ==========================================
        // CONTEXTO DE BASE DE DATOS
        // ==========================================
        private readonly ApplicationDbContext _db;

        public ReportesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // REPORTE GENERAL
        // ==========================================
        public IActionResult Index()
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            // Estadísticas generales
            ViewBag.TotalVentas = _db.Pedidos.Sum(x => x.Total);

            ViewBag.TotalPedidos = _db.Pedidos.Count();

            ViewBag.PromedioVenta = _db.Pedidos.Any()
                ? _db.Pedidos.Average(x => x.Total)
                : 0;

            ViewBag.MesasOcupadas = _db.Mesas.Count(x => x.Estado == "Ocupada");

            // Últimos 10 pedidos registrados
            var ultimosPedidos = _db.Pedidos
                .OrderByDescending(x => x.Fecha)
                .Take(10)
                .ToList();

            return View(ultimosPedidos);
        }
    }
}