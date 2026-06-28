using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Mesa
    {
        public int Id { get; set; }

        public int Numero { get; set; }

        public string Estado { get; set; } = "Disponible";
        // Disponible / Cliente realizando pedido / Pedido en cocina / Pedido listo / Atendido / Ocupada

        public int Capacidad { get; set; } = 4;

        [StringLength(100)]
        public string? Cliente { get; set; }

        public int? Personas { get; set; }

        public DateTime? HoraIngreso { get; set; }
    }
}