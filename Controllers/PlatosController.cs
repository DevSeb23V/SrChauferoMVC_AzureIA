using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Models;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class PlatosController : BaseController
    {
        // ==========================================
        // CONTEXTO DE BASE DE DATOS
        // ==========================================
        private readonly ApplicationDbContext _db;

        public PlatosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // LISTADO DE PLATOS
        // ==========================================
        public IActionResult Index()
        {
            var auth = RequireLogin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (IsCliente())
            {
                var tipoPedido = HttpContext.Session.GetString("TipoPedido");
                if (string.IsNullOrWhiteSpace(tipoPedido))
                {
                    TempData["Error"] = "Primero indica si vas a comer en el local o si tu pedido es para llevar.";
                    return RedirectToAction("Index", "Mesas");
                }

                if (tipoPedido == "Presencial" && HttpContext.Session.GetInt32("MesaSeleccionada") == null)
                {
                    TempData["Error"] = "Para comer en el local primero debes elegir una mesa.";
                    return RedirectToAction("Index", "Mesas");
                }
            }

            var platos = _db.Platos.OrderBy(p => p.Categoria).ToList();

            var carritoJson = HttpContext.Session.GetString("Carrito");
            ViewBag.CarritoJson = carritoJson;

            return View(platos);
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
        // GUARDAR PLATO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Plato plato, IFormFile? imagen)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (ModelState.IsValid)
            {
                if( imagen != null && imagen.Length > 0)
                {
                    string carpeta = Path.Combine(Directory.GetCurrentDirectory(),
                        
                        "wwwroot/img/platos"
                        );

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);

                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        imagen.CopyTo(stream);
                    }

                    plato.ImagenUrl = "/img/platos" + nombreArchivo;

                }
                _db.Platos.Add(plato);
                _db.SaveChanges();

                TempData["Ok"] = "Plato registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(plato);
        }

        // ==========================================
        // FORMULARIO DE EDICIÓN
        // ==========================================
        public IActionResult Edit(int? id)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (id == null || id == 0)
            {
                return NotFound();
            }

            var plato = _db.Platos.Find(id);

            if (plato == null)
            {
                return NotFound();
            }

            return View(plato);
        }

        // ==========================================
        // ACTUALIZAR PLATO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Plato plato, IFormFile? imagen)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (ModelState.IsValid)
            {
                var platoBD = _db.Platos.Find(plato.Id);

                if (platoBD == null)
                {
                    return NotFound();
                }

                platoBD.Nombre = plato.Nombre;
                platoBD.Categoria = plato.Categoria;
                platoBD.Precio = plato.Precio;
                platoBD.Disponible = plato.Disponible;

                if (imagen != null && imagen.Length > 0)
                {
                    string carpeta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/img/platos"
                    );

                    if (!Directory.Exists(carpeta))
                    {
                        Directory.CreateDirectory(carpeta);
                    }

                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);

                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        imagen.CopyTo(stream);
                    }

                    platoBD.ImagenUrl = "/img/platos/" + nombreArchivo;
                }

                _db.Platos.Update(platoBD);
                _db.SaveChanges();

                TempData["Ok"] = "Plato actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(plato);
        }

        // ==========================================
        // FORMULARIO DE ELIMINACIÓN
        // ==========================================
        public IActionResult Delete(int? id)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            if (id == null || id == 0)
            {
                return NotFound();
            }

            var plato = _db.Platos.Find(id);

            if (plato == null)
            {
                return NotFound();
            }

            return View(plato);
        }

        // ==========================================
        // ELIMINAR PLATO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int? id)
        {
            var auth = RequireAdmin();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            var plato = _db.Platos.Find(id);

            if (plato == null)
            {
                return NotFound();
            }

            _db.Platos.Remove(plato);
            _db.SaveChanges();

            TempData["Ok"] = "Plato eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}