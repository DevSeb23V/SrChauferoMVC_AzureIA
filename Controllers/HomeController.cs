using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string filtro = "total", DateTime? fecha = null)
        {
            var auth = RequireAdmin();
            if (auth is not EmptyResult) return auth;

            var pedidos = _db.Pedidos.AsQueryable();

            if (filtro == "hoy")
            {
                var inicio = DateTime.Today;
                var fin = inicio.AddDays(1);

                pedidos = pedidos.Where(p =>
                    p.Fecha >= inicio &&
                    p.Fecha < fin);
            }

            if (filtro == "semana")
            {
                pedidos = pedidos.Where(p =>
                    p.Fecha >= DateTime.Today.AddDays(-7));
            }

            if (filtro == "mes")
            {
                pedidos = pedidos.Where(p =>
                    p.Fecha >= DateTime.Today.AddMonths(-1));
            }

            if (filtro == "fecha" && fecha.HasValue)
            {
                var inicio = fecha.Value.Date;
                var fin = inicio.AddDays(1);

                pedidos = pedidos.Where(p =>
                    p.Fecha >= inicio &&
                    p.Fecha < fin);
            }

            var lista = pedidos
                    .OrderByDescending(p => p.Fecha)
                    .Take(500)
                    .ToList();

            ViewBag.Filtro = filtro;
            ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");

            ViewBag.TotalVentas = lista.Sum(x => x.Total);
            ViewBag.TotalPedidos = lista.Count;
            ViewBag.PromedioVenta = lista.Any() ? lista.Average(x => x.Total) : 0;
            ViewBag.PedidosCocina = _db.Pedidos.Count(x => x.EstadoPedido == "EnviadoCocina" || x.EstadoPedido == "Cocinandose");
            ViewBag.PedidosListos = _db.Pedidos.Count(x => x.EstadoPedido == "Listo");
            ViewBag.MesasOcupadas = _db.Mesas.Count(x => x.Estado != "Disponible");
            ViewBag.MesasTotal = _db.Mesas.Count();

            ViewBag.QR = lista.Count(x => x.MetodoPago == "QR");
            ViewBag.Efectivo = lista.Count(x => x.MetodoPago == "Efectivo");

            ViewBag.UltimosPedidos = _db.Pedidos
                .OrderByDescending(x => x.Fecha)
                .Take(8)
                .ToList();

            return View();
        }
    }
}