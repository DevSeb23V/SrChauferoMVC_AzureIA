using System.Text;
using System.Text.Json;
using SrChauferoMVC_AzureIA.Data;

namespace SrChauferoMVC_AzureIA.Services
{
    public class AzureOpenAIService : IIAService
    {
        // ==========================================
        // DEPENDENCIAS
        // ==========================================
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public AzureOpenAIService(
            HttpClient http,
            IConfiguration config,
            ApplicationDbContext db
        )
        {
            _http = http;
            _config = config;
            _db = db;
        }

        // ==========================================
        // RECOMENDACIÓN CON IA
        // ==========================================
        public async Task<string> RecomendarAsync(string texto)
        {
            texto ??= "";

            string textoLower = texto.ToLower();

            // Respuesta local para consultas sobre mesas
            if (
                textoLower.Contains("mesa") &&
                (
                    textoLower.Contains("ocupad") ||
                    textoLower.Contains("cuánt") ||
                    textoLower.Contains("cuant") ||
                    textoLower.Contains("porcentaje")
                )
            )
            {
                int totalMesas = _db.Mesas.Count();
                int ocupadas = _db.Mesas.Count(m => m.Estado == "Ocupada");
                int libres = _db.Mesas.Count(m => m.Estado == "Libre");

                decimal porcentaje = totalMesas == 0
                    ? 0
                    : Math.Round((decimal)ocupadas * 100 / totalMesas, 2);

                return $"Actualmente hay {ocupadas} mesas ocupadas y {libres} mesas libres. " +
                       $"El porcentaje de ocupación del restaurante es {porcentaje}%.";
            }

            // Configuración de Gemini
            string? apiKey = _config["Gemini:ApiKey"];

            string endpoint = _config["Gemini:Endpoint"]
                ?? "https://generativelanguage.googleapis.com";

            string model = _config["Gemini:Model"]
                ?? "gemini-2.0-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Falta configurar la API Key de Gemini en appsettings.json.";
            }

            string url =
                $"{endpoint.TrimEnd('/')}/v1beta/models/{model}:generateContent?key={apiKey}";

            // Datos actuales de mesas
            var mesas = _db.Mesas
                .OrderBy(m => m.Numero)
                .Select(m => new
                {
                    m.Numero,
                    m.Estado,
                    m.Cliente,
                    m.Personas,
                    HoraIngreso = m.HoraIngreso == null
                        ? ""
                        : m.HoraIngreso.Value.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            // Prompt enviado a la IA
            string prompt = $"""
                Eres un asistente de IA para el restaurante peruano Sr. Chaufero.
                Responde de forma breve, clara y útil.
                Puedes recomendar promociones, platos, ventas y también analizar el estado de las mesas.

                Datos actuales de mesas:
                {JsonSerializer.Serialize(mesas)}

                Consulta del usuario:
                {texto}
                """;

            // Cuerpo JSON para Gemini
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            // Enviar solicitud HTTP
            var response = await _http.PostAsync(
                url,
                new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            string json = await response.Content.ReadAsStringAsync();

            // Validar respuesta del servicio
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 503)
                {
                    return "El servicio de IA está ocupado temporalmente. Intenta nuevamente en unos segundos.";
                }

                return $"No se pudo consumir Gemini. Código: {(int)response.StatusCode}. " +
                       "Revisa la API Key y el modelo configurado.";
            }

            // Leer respuesta de Gemini
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Sin respuesta.";
        }
    }
}