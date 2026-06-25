using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Insumo
    {
        // ==========================================
        // IDENTIFICADOR
        // ==========================================
        public int Id { get; set; }

        // ==========================================
        // DATOS DEL INSUMO
        // ==========================================
        [Required]
        public string Nombre { get; set; } = "";

        public int Stock { get; set; }

        public int StockMinimo { get; set; }

        public string Unidad { get; set; } = "und";
    }
}