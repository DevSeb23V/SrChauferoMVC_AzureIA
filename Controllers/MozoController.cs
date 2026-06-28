using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class MozoController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public MozoController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var pedidos = _db.Pedidos
                .Include(p => p.Detalles)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pedidos);
        }

        public IActionResult Detalle(int id)
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var pedido = _db.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PedidoPagado(int id)
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var pedido = _db.Pedidos.Find(id);
            if (pedido == null) return NotFound();

            pedido.EstadoPago = "Pagado";

            _db.SaveChanges();

            TempData["Ok"] = $"Pedido #{pedido.Id} marcado como pagado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtenderPedido(int id)
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var pedido = _db.Pedidos.Find(id);
            if (pedido == null) return NotFound();

            pedido.EstadoPedido = "Atendido";

            if (pedido.Mesa != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(m => m.Numero == pedido.Mesa.Value);
                if (mesa != null)
                {
                    mesa.Estado = "Atendido";
                }
            }

            _db.SaveChanges();

            TempData["Ok"] = $"Pedido #{pedido.Id} atendido correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}