using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ingrese su nombre completo.")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "Ingrese un usuario.")]
        public string Usuario { get; set; } = "";

        [Required(ErrorMessage = "Ingrese una contraseña.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirme su contraseña.")]
        public string ConfirmarPassword { get; set; } = "";

        public string? Error { get; set; }
    }
}
