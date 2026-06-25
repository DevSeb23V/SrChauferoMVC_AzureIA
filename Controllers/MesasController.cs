using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class MesasController : BaseController
    {
        // ==========================================
        // CONTEXTO DE BASE DE DATOS
        // ==========================================
        private readonly ApplicationDbContext _db;

        public MesasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // LISTADO DE MESAS
        // ==========================================
        public IActionResult Index()
        {
            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var mesas = _db.Mesas
                .OrderBy(m => m.Numero)
                .ToList();

            return View(mesas);
        }

        // ==========================================
        // OCUPAR MESA
        // ==========================================
        [HttpPost]
        public IActionResult Ocupar(int id, string cliente, int personas)
        {
            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var mesa = _db.Mesas.Find(id);

            if (mesa == null)
            {
                return NotFound();
            }

            // Validar nombre del cliente
            if (string.IsNullOrWhiteSpace(cliente))
            {
                TempData["Error"] =
                    "Debe ingresar el nombre del cliente.";

                return RedirectToAction(nameof(Index));
            }

            // Validar cantidad de personas
            if (personas <= 0 || personas > mesa.Capacidad)
            {
                TempData["Error"] =
                    $"La cantidad de personas debe estar entre 1 y {mesa.Capacidad}.";

                return RedirectToAction(nameof(Index));
            }

            // Actualizar datos de la mesa
            mesa.Estado = "Ocupada";
            mesa.Cliente = cliente.Trim();
            mesa.Personas = personas;
            mesa.HoraIngreso = DateTime.Now;

            _db.SaveChanges();

            TempData["Ok"] =
                $"Mesa {mesa.Numero} ocupada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // LIBERAR MESA
        // ==========================================
        [HttpPost]
        public IActionResult Liberar(int id)
        {
            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var mesa = _db.Mesas.Find(id);

            if (mesa == null)
            {
                return NotFound();
            }

            // Limpiar datos de la mesa
            mesa.Estado = "Libre";
            mesa.Cliente = null;
            mesa.Personas = 0;
            mesa.HoraIngreso = null;

            _db.SaveChanges();

            TempData["Ok"] =
                $"Mesa {mesa.Numero} liberada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}