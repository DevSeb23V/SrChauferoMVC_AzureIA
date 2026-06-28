namespace SrChauferoMVC_AzureIA.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Cliente { get; set; } = "Cliente";

        public int? Mesa { get; set; }

        public string TipoPedido { get; set; } = "Presencial";
        // Presencial / Para llevar

        public decimal Total { get; set; }

        public string EstadoPedido { get; set; } = "PendientePago";
        // PendientePago / EnviadoCocina / Cocinandose / Listo / Atendido / Cancelado

        public string EstadoPago { get; set; } = "Pendiente";
        // Pendiente / Pagado
        public string MetodoPago { get; set; } = "QR";
        // QR / Efectivo

        public List<DetallePedido> Detalles { get; set; } = new();
    }
}