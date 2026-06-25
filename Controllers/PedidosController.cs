using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class PedidosController : BaseController
    {
        // ==========================================
        // CONTEXTO DE BASE DE DATOS
        // ==========================================
        private readonly ApplicationDbContext _db;

        public PedidosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // LISTADO DE PEDIDOS
        // ==========================================
        public IActionResult Index()
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var pedidos = _db.Pedidos
                .OrderByDescending(x => x.Fecha)
                .ToList();

            return View(pedidos);
        }

        // ==========================================
        // FORMULARIO DE REGISTRO
        // ==========================================
        public IActionResult Create()
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            return View();
        }

        // ==========================================
        // GUARDAR PEDIDO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pedido pedido)
        {
            pedido.Fecha = DateTime.Now;

            _db.Pedidos.Add(pedido);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}