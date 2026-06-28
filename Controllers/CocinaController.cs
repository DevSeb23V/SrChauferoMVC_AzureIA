using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class CocinaController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public CocinaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var auth = RequireCocinero();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var pedidos = _db.Pedidos
                .Include(p => p.Detalles)
                .Where(p =>
                    p.EstadoPedido == "EnviadoCocina" ||
                    p.EstadoPedido == "Cocinandose"
                )
                .OrderBy(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AceptarPedido(int id)
        {
            var auth = RequireCocinero();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var pedido = _db.Pedidos.Find(id);

            if (pedido == null)
            {
                return NotFound();
            }

            pedido.EstadoPedido = "Cocinandose";

            if (pedido.Mesa != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(m => m.Numero == pedido.Mesa.Value);

                if (mesa != null)
                {
                    mesa.Estado = "Pedido en cocina";
                }
            }

            _db.SaveChanges();

            TempData["Ok"] = $"Pedido #{pedido.Id} aceptado. Ahora está cocinándose.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PedidoListo(int id)
        {
            var auth = RequireCocinero();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var pedido = _db.Pedidos.Find(id);

            if (pedido == null)
            {
                return NotFound();
            }

            pedido.EstadoPedido = "Listo";

            if (pedido.Mesa != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(m => m.Numero == pedido.Mesa.Value);

                if (mesa != null)
                {
                    mesa.Estado = "Pedido listo";
                }
            }

            _db.SaveChanges();

            TempData["Ok"] = $"Pedido #{pedido.Id} marcado como listo.";

            return RedirectToAction(nameof(Index));
        }
    }
}