using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class ReportesController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public ReportesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string filtro = "total", DateTime? fecha = null)
        {
            var auth = RequireAdmin();
            if (auth is not EmptyResult) return auth;

            var pedidos = _db.Pedidos.AsQueryable();

            if (filtro == "hoy")
                pedidos = pedidos.Where(p => p.Fecha.Date == DateTime.Today);

            if (filtro == "semana")
                pedidos = pedidos.Where(p => p.Fecha >= DateTime.Today.AddDays(-7));

            if (filtro == "mes")
                pedidos = pedidos.Where(p => p.Fecha >= DateTime.Today.AddMonths(-1));

            if (filtro == "fecha" && fecha != null)
                pedidos = pedidos.Where(p => p.Fecha.Date == fecha.Value.Date);

            var lista = pedidos
                .OrderByDescending(p => p.Fecha)
                .ToList();

            ViewBag.TotalVentas = lista.Sum(x => x.Total);
            ViewBag.TotalPedidos = lista.Count;
            ViewBag.PromedioVenta = lista.Any() ? lista.Average(x => x.Total) : 0;
            ViewBag.QR = lista.Count(x => x.MetodoPago == "QR");
            ViewBag.Efectivo = lista.Count(x => x.MetodoPago == "Efectivo");
            ViewBag.Filtro = filtro;
            ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");

            return View(lista);
        }
    }
}