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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTipoPedido(string tipo)
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsUsuario()) return RedirectToAction("Index", "Home");

            tipo = (tipo ?? "").Trim();

            if (tipo != "Presencial" && tipo != "Para llevar")
            {
                TempData["Error"] = "Selecciona si el pedido será para comer en el local o para llevar.";
                return RedirectToAction("Index", "Mesas");
            }

            HttpContext.Session.SetString("TipoPedido", tipo);

            if (tipo == "Para llevar")
            {
                HttpContext.Session.Remove("MesaSeleccionada");
                TempData["Ok"] = "Pedido configurado para llevar. Ahora selecciona tus platos.";
                return RedirectToAction("Index", "Platos");
            }

            TempData["Ok"] = "Pedido configurado para comer en el local. Selecciona una mesa libre.";
            return RedirectToAction("Index", "Mesas");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ElegirMesa(int id)
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsUsuario()) return RedirectToAction("Index", "Mesas");

            HttpContext.Session.SetString("TipoPedido", "Presencial");

            var mesa = _db.Mesas.Find(id);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa seleccionada no existe.";
                return RedirectToAction("Index", "Mesas");
            }

            if (mesa.Estado != "Libre")
            {
                TempData["Error"] = "La mesa seleccionada no está disponible.";
                return RedirectToAction("Index", "Mesas");
            }

            HttpContext.Session.SetInt32("MesaSeleccionada", mesa.Numero);
            TempData["Ok"] = $"Seleccionaste la mesa {mesa.Numero}. Ahora elige tus platos.";
            return RedirectToAction("Index", "Platos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarPlato(int platoId, int cantidad = 1)
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsUsuario()) return RedirectToAction("Index", "Platos");

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
            TempData["Ok"] = $"{plato.Nombre} agregado a tu pedido.";
            return RedirectToAction("Index", "Platos");
        }

        public IActionResult Pedido()
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsUsuario()) return RedirectToAction("Index", "Home");

            ViewBag.MesaSeleccionada = HttpContext.Session.GetInt32("MesaSeleccionada");
            ViewBag.TipoPedido = HttpContext.Session.GetString("TipoPedido");
            return View(ObtenerCarrito());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Quitar(int platoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.PlatoId == platoId);

            if (item != null)
            {
                item.Cantidad--;
                if (item.Cantidad <= 0) carrito.Remove(item);
                GuardarCarrito(carrito);
            }

            return RedirectToAction(nameof(Pedido));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pagar()
        {
            var auth = RequireLogin();
            if (auth is not EmptyResult) return auth;

            if (!IsUsuario()) return RedirectToAction("Index", "Home");

            var tipoPedido = HttpContext.Session.GetString("TipoPedido");
            if (string.IsNullOrWhiteSpace(tipoPedido))
            {
                TempData["Error"] = "Primero indica si vas a comer en el local o si tu pedido es para llevar.";
                return RedirectToAction("Index", "Mesas");
            }

            var carrito = ObtenerCarrito();
            if (!carrito.Any())
            {
                TempData["Error"] = "Primero debes agregar al menos un plato.";
                return RedirectToAction(nameof(Pedido));
            }

            int mesaNumero = HttpContext.Session.GetInt32("MesaSeleccionada") ?? 0;
            if (tipoPedido == "Presencial" && mesaNumero == 0)
            {
                TempData["Error"] = "Para comer en el local debes elegir una mesa antes de pagar.";
                return RedirectToAction("Index", "Mesas");
            }

            decimal total = carrito.Sum(x => x.Subtotal);
            string cliente = HttpContext.Session.GetString("Nombre") ?? HttpContext.Session.GetString("Usuario") ?? "Cliente";

            var pedido = new Pedido
            {
                Cliente = cliente,
                Mesa = mesaNumero,
                Fecha = DateTime.Now,
                Total = total,
                Estado = $"Pago QR generado - {tipoPedido}"
            };

            _db.Pedidos.Add(pedido);

            if (mesaNumero > 0)
            {
                var mesa = _db.Mesas.FirstOrDefault(m => m.Numero == mesaNumero);
                if (mesa != null && mesa.Estado == "Libre")
                {
                    mesa.Estado = "Ocupada";
                    mesa.Cliente = cliente;
                    mesa.Personas = 1;
                    mesa.HoraIngreso = DateTime.Now;
                }
            }

            _db.SaveChanges();

            string codigoPago = $"SRCH-{pedido.Id:D5}-{DateTime.Now:HHmmss}";
            string referencia = tipoPedido == "Presencial" ? $"Mesa {mesaNumero}" : "Pedido para llevar";
            string textoQr = $"Sr. Chaufero | Pedido {pedido.Id} | {referencia} | Total S/ {total:0.00} | Codigo {codigoPago}";

            ViewBag.PedidoId = pedido.Id;
            ViewBag.CodigoPago = codigoPago;
            ViewBag.Total = total;
            ViewBag.Mesa = mesaNumero;
            ViewBag.TipoPedido = tipoPedido;
            ViewBag.QrSvg = GenerarQrSimulado(textoQr);
            ViewBag.Items = carrito;

            HttpContext.Session.Remove(CarritoKey);
            HttpContext.Session.Remove("MesaSeleccionada");
            HttpContext.Session.Remove("TipoPedido");

            return View("Pago");
        }

        private List<CarritoItemViewModel> ObtenerCarrito()
        {
            var json = HttpContext.Session.GetString(CarritoKey);
            if (string.IsNullOrWhiteSpace(json)) return new List<CarritoItemViewModel>();

            return JsonSerializer.Deserialize<List<CarritoItemViewModel>>(json) ?? new List<CarritoItemViewModel>();
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
            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{size * cell}' height='{size * cell}' viewBox='0 0 {size * cell} {size * cell}' role='img' aria-label='QR de pago simulado'>");
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
                    bool inFinder = (x >= 1 && x <= 7 && y >= 1 && y <= 7) ||
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
