using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Mesa
    {
        // ==========================================
        // IDENTIFICADOR
        // ==========================================
        public int Id { get; set; }

        // ==========================================
        // DATOS DE LA MESA
        // ==========================================
        public int Numero { get; set; }

        public string Estado { get; set; } = "Libre";

        public int Capacidad { get; set; } = 4;

        // ==========================================
        // DATOS DEL CLIENTE
        // ==========================================
        [StringLength(100)]
        public string? Cliente { get; set; }

        public int? Personas { get; set; }

        public DateTime? HoraIngreso { get; set; }
    }
}