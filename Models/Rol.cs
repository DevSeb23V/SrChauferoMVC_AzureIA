using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Rol
    {
        [Key]
        public int RolId { get; set; }

        [Required]
        public string Nombre { get; set; }


        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}