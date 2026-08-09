using Microsoft.AspNetCore.Mvc;
using SrChauferoMVC_AzureIA.Services;

namespace SrChauferoMVC_AzureIA.Controllers
{
    public class IAController : BaseController
    {
        // ==========================================
        // SERVICIO DE INTELIGENCIA ARTIFICIAL
        // ==========================================
        private readonly IIAService _ia;

        public IAController(IIAService ia)
        {
            _ia = ia;
        }

        // ==========================================
        // VISTA PRINCIPAL DE IA (GET)
        // ==========================================
        public IActionResult Index()
        {
            // Validar sesión
            var auth = RequireIA();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            return View();
        }

        // ==========================================
        // CONSULTAR IA (POST)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Index(string consulta)
        {
            var auth = RequireIA();

            if (auth is not EmptyResult)
            {
                return auth;
            }

            ViewBag.Respuesta = await _ia.RecomendarAsync(
                consulta ?? "Recomienda promociones para Sr Chaufero"
            );

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var respuesta = await _ia.RecomendarAsync(request.Mensaje ?? "");
            return Json(new { respuesta });
        }

        public class ChatRequest
        {
            public string? Mensaje { get; set; }
        }
    }
}