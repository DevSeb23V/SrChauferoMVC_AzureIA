using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Data;
using SrChauferoMVC_AzureIA.Models;
using SrChauferoMVC_AzureIA.ViewModels;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class ClienteController : BaseController
    {
        private const string CarritoKey = "Carrito";
        private readonly ApplicationDbContext _db;

        public ClienteController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            HttpContext.Session.Remove("TipoPedido");
            HttpContext.Session.Remove("MesaSeleccionada");
            HttpContext.Session.Remove(CarritoKey);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTipoPedido(string tipo)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            tipo = (tipo ?? "").Trim();

            if (tipo != "Presencial" && tipo != "Para llevar")
            {
                TempData["Error"] = "Selecciona una modalidad válida.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString("TipoPedido", tipo);

            if (tipo == "Para llevar")
            {
                HttpContext.Session.Remove("MesaSeleccionada");
                return RedirectToAction("Index", "Platos");
            }

            return RedirectToAction("Index", "Mesas");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ElegirMesa(int id)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var mesa = _db.Mesas.Find(id);

            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index", "Mesas");
            }

            int? mesaSeleccionada = HttpContext.Session.GetInt32("MesaSeleccionada");

            if (mesaSeleccionada == mesa.Numero)
            {
                TempData["Ok"] = $"Continuas con la mesa {mesa.Numero}.";
                return RedirectToAction("Index", "Platos");
            }

            if (mesa.Estado != "Disponible")
            {
                TempData["Error"] = "La mesa no está disponible.";
                return RedirectToAction("Index", "Mesas");
            }

            mesa.Estado = "Cliente realizando pedido";
            mesa.Cliente = "Cliente";
            mesa.Personas = 1;
            mesa.HoraIngreso = DateTime.Now;

            _db.SaveChanges();

            HttpContext.Session.SetString("TipoPedido", "Presencial");
            HttpContext.Session.SetInt32("MesaSeleccionada", mesa.Numero);

            TempData["Ok"] = $"Seleccionaste la mesa {mesa.Numero}. Ahora elige tus platos.";
            return RedirectToAction("Index", "Platos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarPlato(int platoId, int cantidad = 1)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var tipoPedido = HttpContext.Session.GetString("TipoPedido");

            if (string.IsNullOrWhiteSpace(tipoPedido))
            {
                TempData["Error"] = "Primero selecciona si tu pedido será para mesa o para llevar.";
                return RedirectToAction(nameof(Index));
            }

            if (tipoPedido == "Presencial" && HttpContext.Session.GetInt32("MesaSeleccionada") == null)
            {
                TempData["Error"] = "Primero selecciona una mesa.";
                return RedirectToAction("Index", "Mesas");
            }

            cantidad = Math.Clamp(cantidad, 1, 20);

            var plato = _db.Platos.Find(platoId);

            if (plato == null || !plato.Disponible)
            {
                TempData["Error"] = "El plato seleccionado no está disponible.";
                return RedirectToAction("Index", "Platos");
            }

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.PlatoId == plato.Id);

            if (item == null)
            {
                carrito.Add(new CarritoItemViewModel
                {
                    PlatoId = plato.Id,
                    Nombre = plato.Nombre,
                    Precio = plato.Precio,
                    Cantidad = cantidad
                });
            }
            else
            {
                item.Cantidad += cantidad;
            }

            GuardarCarrito(carrito);

            TempData["Ok"] = $"{plato.Nombre} agregado al pedido.";
            return RedirectToAction("Index", "Platos");
        }

        public IActionResult Pedido()
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            ViewBag.TipoPedido = HttpContext.Session.GetString("TipoPedido");
            ViewBag.MesaSeleccionada = HttpContext.Session.GetInt32("MesaSeleccionada");

            return View(ObtenerCarrito());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Quitar(int platoId)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.PlatoId == platoId);

            if (item != null)
            {
                item.Cantidad--;

                if (item.Cantidad <= 0)
                {
                    carrito.Remove(item);
                }

                GuardarCarrito(carrito);
            }

            return RedirectToAction(nameof(Pedido));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarItem(int platoId, int cantidad)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            cantidad = Math.Clamp(cantidad, 1, 20);

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.PlatoId == platoId);

            if (item != null)
            {
                item.Cantidad = cantidad;
                GuardarCarrito(carrito);
                TempData["Ok"] = "Pedido actualizado.";
            }

            return RedirectToAction("Index", "Platos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarItem(int platoId)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.PlatoId == platoId);

            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);
                TempData["Ok"] = "Plato eliminado del pedido.";
            }

            return RedirectToAction("Index", "Platos");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pagar(string nombreCliente)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var tipoPedido = HttpContext.Session.GetString("TipoPedido");
            var carrito = ObtenerCarrito();

            if (string.IsNullOrWhiteSpace(tipoPedido) || !carrito.Any())
            {
                TempData["Error"] = "Tu pedido está incompleto.";
                return RedirectToAction(nameof(Pedido));
            }

            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                TempData["Error"] = "Ingresa tu nombre antes de continuar.";
                return RedirectToAction(nameof(Pedido));
            }

            int? mesaNumero = HttpContext.Session.GetInt32("MesaSeleccionada");
            decimal total = carrito.Sum(x => x.Subtotal);

            HttpContext.Session.SetString("NombreClientePedido", nombreCliente.Trim());

            string codigoPago = $"SRCH-TEMP-{DateTime.Now:HHmmss}";
            string referencia = tipoPedido == "Presencial" ? $"Mesa {mesaNumero}" : "Para llevar";
            string textoQr = $"Sr. Chaufero | {referencia} | Total S/ {total:0.00}";

            ViewBag.CodigoPago = codigoPago;
            ViewBag.Total = total;
            ViewBag.Mesa = mesaNumero;
            ViewBag.TipoPedido = tipoPedido;
            ViewBag.QrSvg = GenerarQrSimulado(textoQr);
            ViewBag.Items = carrito;

            return View("Pago");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarPago(string metodoPago)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var tipoPedido = HttpContext.Session.GetString("TipoPedido");
            var nombreCliente = HttpContext.Session.GetString("NombreClientePedido");
            var carrito = ObtenerCarrito();

            if (string.IsNullOrWhiteSpace(tipoPedido) || string.IsNullOrWhiteSpace(nombreCliente) || !carrito.Any())
            {
                TempData["Error"] = "No se pudo confirmar el pedido.";
                return RedirectToAction(nameof(Pedido));
            }

            int? mesaNumero = HttpContext.Session.GetInt32("MesaSeleccionada");
            decimal total = carrito.Sum(x => x.Subtotal);

            var pedido = new Pedido
            {
                Cliente = nombreCliente,
                Mesa = tipoPedido == "Presencial" ? mesaNumero : null,
                TipoPedido = tipoPedido,
                Fecha = DateTime.Now,
                Total = total,
                EstadoPago = metodoPago == "Efectivo" ? "Pendiente" : "Pagado",
                MetodoPago = metodoPago,
                EstadoPedido = "EnviadoCocina",
                Detalles = carrito.Select(x => new DetallePedido
                {
                    PlatoId = x.PlatoId,
                    NombrePlato = x.Nombre,
                    Cantidad = x.Cantidad,
                    PrecioUnitario = x.Precio
                }).ToList()
            };

            _db.Pedidos.Add(pedido);

            if (tipoPedido == "Presencial" && mesaNumero != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(x => x.Numero == mesaNumero.Value);

                if (mesa != null)
                {
                    mesa.Estado = "Pedido en cocina";
                    mesa.Cliente = nombreCliente;
                    mesa.HoraIngreso = DateTime.Now;
                }
            }

            _db.SaveChanges();

            HttpContext.Session.Remove("Carrito");
            HttpContext.Session.Remove("MesaSeleccionada");
            HttpContext.Session.Remove("TipoPedido");
            HttpContext.Session.Remove("NombreClientePedido");

            TempData["Ok"] = "Pedido enviado a cocina correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelarPedido()
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            int? mesaNumero = HttpContext.Session.GetInt32("MesaSeleccionada");

            if (mesaNumero != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(x => x.Numero == mesaNumero.Value);

                if (mesa != null && mesa.Estado == "Cliente realizando pedido")
                {
                    mesa.Estado = "Disponible";
                    mesa.Cliente = null;
                    mesa.Personas = null;
                    mesa.HoraIngreso = null;
                }
            }

            HttpContext.Session.Remove("Carrito");
            HttpContext.Session.Remove("MesaSeleccionada");
            HttpContext.Session.Remove("TipoPedido");

            _db.SaveChanges();

            TempData["Ok"] = "Pedido cancelado correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelarPedidoRegistrado(int pedidoId)
        {
            var auth = RequireCliente();
            if (auth is not EmptyResult) return auth;

            var pedido = _db.Pedidos.Find(pedidoId);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index", "Cliente");
            }

            pedido.EstadoPedido = "Cancelado";

            if (pedido.Mesa != null)
            {
                var mesa = _db.Mesas.FirstOrDefault(x => x.Numero == pedido.Mesa.Value);

                if (mesa != null)
                {
                    mesa.Estado = "Disponible";
                    mesa.Cliente = null;
                    mesa.Personas = null;
                    mesa.HoraIngreso = null;
                }
            }

            _db.SaveChanges();

            TempData["Ok"] = "Pedido cancelado correctamente.";
            return RedirectToAction("Index", "Cliente");
        }

        private List<CarritoItemViewModel> ObtenerCarrito()
        {
            var json = HttpContext.Session.GetString(CarritoKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CarritoItemViewModel>();
            }

            return JsonSerializer.Deserialize<List<CarritoItemViewModel>>(json)
                ?? new List<CarritoItemViewModel>();
        }

        private void GuardarCarrito(List<CarritoItemViewModel> carrito)
        {
            HttpContext.Session.SetString(CarritoKey, JsonSerializer.Serialize(carrito));
        }

        private static string GenerarQrSimulado(string texto)
        {
            const int size = 25;
            const int cell = 8;

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            var sb = new StringBuilder();

            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{size * cell}' height='{size * cell}' viewBox='0 0 {size * cell} {size * cell}'>");
            sb.Append("<rect width='100%' height='100%' fill='white'/>");

            void Finder(int x, int y)
            {
                sb.Append($"<rect x='{x * cell}' y='{y * cell}' width='{7 * cell}' height='{7 * cell}' fill='black'/>");
                sb.Append($"<rect x='{(x + 1) * cell}' y='{(y + 1) * cell}' width='{5 * cell}' height='{5 * cell}' fill='white'/>");
                sb.Append($"<rect x='{(x + 2) * cell}' y='{(y + 2) * cell}' width='{3 * cell}' height='{3 * cell}' fill='black'/>");
            }

            Finder(1, 1);
            Finder(17, 1);
            Finder(1, 17);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inFinder =
                        (x >= 1 && x <= 7 && y >= 1 && y <= 7) ||
                        (x >= 17 && x <= 23 && y >= 1 && y <= 7) ||
                        (x >= 1 && x <= 7 && y >= 17 && y <= 23);

                    if (inFinder) continue;

                    int index = (x + y * size) % hash.Length;
                    bool dark = ((hash[index] + x * 17 + y * 31) % 3) == 0;

                    if (dark)
                    {
                        sb.Append($"<rect x='{x * cell}' y='{y * cell}' width='{cell}' height='{cell}' fill='black'/>");
                    }
                }
            }

            sb.Append("</svg>");
            return sb.ToString();
        }
    }
}