using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SrChauferoMVC_AzureIA.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }


        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }


        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string NombreUsuario { get; set; }


        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }


        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        public string Correo { get; set; }


        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public int? RolId { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol? Rol { get; set; }
    }
}