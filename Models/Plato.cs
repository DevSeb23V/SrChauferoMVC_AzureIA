using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Plato
    {
        // ==========================================
        // IDENTIFICADOR
        // ==========================================
        public int Id { get; set; }

        // ==========================================
        // INFORMACIÓN DEL PLATO
        // ==========================================
        [Required(ErrorMessage = "El nombre del plato es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = "";

        [Range(0.01, 9999, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        public string ImagenUrl { get; set; } = "";

        public bool Disponible { get; set; } = true;
    }
}