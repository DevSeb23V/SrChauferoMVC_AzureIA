namespace SrChauferoMVC_AzureIA.Models
{
    public class Pedido
    {
        // ==========================================
        // IDENTIFICADOR
        // ==========================================
        public int Id { get; set; }

        // ==========================================
        // INFORMACIÓN DEL PEDIDO
        // ==========================================
        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Cliente { get; set; } = "Consumidor final";

        public int Mesa { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = "Registrado";
    }
}