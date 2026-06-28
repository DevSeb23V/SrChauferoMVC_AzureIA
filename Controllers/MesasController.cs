using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class MesasController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public MesasController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            var mesas = _db.Mesas
                .OrderBy(m => m.Numero)
                .ToList();

            ViewBag.Delivery = _db.Pedidos
                .Where(p => p.TipoPedido == "Para llevar" && p.EstadoPedido != "Cancelado" && p.EstadoPedido != "Atendido")
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(mesas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ocupar(int id, string cliente, int personas)
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var mesa = _db.Mesas.Find(id);
            if (mesa == null) return NotFound();

            if (string.IsNullOrWhiteSpace(cliente))
            {
                TempData["Error"] = "Debe ingresar el nombre del cliente.";
                return RedirectToAction(nameof(Index));
            }

            if (personas <= 0 || personas > mesa.Capacidad)
            {
                TempData["Error"] = $"La cantidad de personas debe estar entre 1 y {mesa.Capacidad}.";
                return RedirectToAction(nameof(Index));
            }

            mesa.Estado = "Ocupada";
            mesa.Cliente = cliente.Trim();
            mesa.Personas = personas;
            mesa.HoraIngreso = DateTime.Now;

            _db.SaveChanges();

            TempData["Ok"] = $"Mesa {mesa.Numero} ocupada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Liberar(int id)
        {
            var auth = RequireMozo();
            if (auth is not EmptyResult) return auth;

            var mesa = _db.Mesas.Find(id);
            if (mesa == null) return NotFound();

            mesa.Estado = "Disponible";
            mesa.Cliente = null;
            mesa.Personas = null;
            mesa.HoraIngreso = null;

            _db.SaveChanges();

            TempData["Ok"] = $"Mesa {mesa.Numero} liberada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}