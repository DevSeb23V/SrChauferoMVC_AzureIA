using System.ComponentModel.DataAnnotations;

namespace SrChauferoMVC_AzureIA.ViewModels
{
    public class LoginViewModel
    {
        // ==========================================
        // CREDENCIALES DE ACCESO
        // ==========================================
        [Required]
        public string Usuario { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        // ==========================================
        // CAPTCHA
        // ==========================================
        [Required]
        public int CaptchaRespuesta { get; set; }

        // ==========================================
        // MENSAJES DE ERROR
        // ==========================================
        public string? Error { get; set; }
    }
}