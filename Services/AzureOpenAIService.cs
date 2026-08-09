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

            texto = texto?.Trim() ?? "";


            if (string.IsNullOrWhiteSpace(texto))
            {
                return "Hola 👋, escribe una consulta para poder ayudarte.";
            }



            string textoLower = texto.ToLower();



            // ==========================================
            // RESPUESTA LOCAL DE MESAS
            // ==========================================

            if (
                textoLower.Contains("mesa") &&
                (
                    textoLower.Contains("ocupad") ||
                    textoLower.Contains("libre") ||
                    textoLower.Contains("disponible") ||
                    textoLower.Contains("cuánt") ||
                    textoLower.Contains("cuant") ||
                    textoLower.Contains("porcentaje")
                )
            )
            {

                int totalMesas = _db.Mesas.Count();

                int ocupadas = _db.Mesas
                    .Count(m => m.Estado == "Ocupada");


                int libres = _db.Mesas
                    .Count(m => m.Estado == "Libre");


                decimal porcentaje = totalMesas == 0
                    ? 0
                    : Math.Round((decimal)ocupadas * 100 / totalMesas, 2);



                return
                $"Actualmente hay {ocupadas} mesas ocupadas y {libres} mesas libres. " +
                $"La ocupación del restaurante es {porcentaje}%.";
            }





            // ==========================================
            // CONFIGURACIÓN GEMINI
            // ==========================================

            string? apiKey = _config["Gemini:ApiKey"];


            string endpoint = _config["Gemini:Endpoint"]
                ?? "https://generativelanguage.googleapis.com";


            string model = _config["Gemini:Model"]
                ?? "gemini-2.0-flash";



            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "La API Key de Gemini no está configurada.";
            }



            string url =
                $"{endpoint.TrimEnd('/')}/v1beta/models/{model}:generateContent?key={apiKey}";






            // ==========================================
            // DATOS PERMITIDOS PARA IA
            // ==========================================


            object? mesas = null;
            object? platos = null;



            // Solo enviar mesas si pregunta sobre mesas

            if (
                textoLower.Contains("mesa") ||
                textoLower.Contains("ocupada") ||
                textoLower.Contains("libre")
            )
            {

                mesas = _db.Mesas
                    .OrderBy(m => m.Numero)
                    .Select(m => new
                    {
                        Numero = m.Numero,
                        Estado = m.Estado
                    })
                    .ToList();

            }



            // Solo enviar platos si pregunta por comida

            if (
                textoLower.Contains("plato") ||
                textoLower.Contains("comer") ||
                textoLower.Contains("recomienda") ||
                textoLower.Contains("quiero") ||
                textoLower.Contains("categoría") ||
                textoLower.Contains("categoria") ||
                textoLower.Contains("ingrediente") ||
                textoLower.Contains("ingredientes") ||
                textoLower.Contains("lleva") ||
                textoLower.Contains("contiene") ||
                textoLower.Contains("tiene")
            )
            {

                platos = _db.Platos
                    .Where(p => p.Disponible)
                    .Select(p => new
                    {
                        Nombre = p.Nombre,
                        Categoria = p.Categoria
                    })
                    .OrderBy(p => p.Categoria)
                    .ToList();

            }





            // ==========================================
            // PROMPT SEGURO
            // ==========================================


            string prompt =
            $"""
            Eres el asistente virtual oficial del restaurante peruano Sr. Chaufero.

            Tu objetivo es ayudar únicamente con información autorizada.


            ==========================
            REGLAS DE SEGURIDAD
            ==========================

            - No inventes información.
            - No crees platos inexistentes.
            - Solo recomienda platos incluidos en PLATOS DISPONIBLES.
            - Si un plato no aparece en la lista, indica que no está disponible.
            - Nunca reveles datos privados del restaurante.
            - Nunca menciones información de clientes.
            - Nunca muestres datos internos del sistema.


            ==========================
            PLATOS
            ==========================

            Cuando recomiendes comida:

            - Usa únicamente los platos disponibles.
            - Respeta exactamente sus nombres.
            - Puedes recomendar según categoría.


            ==========================
            INGREDIENTES DE LOS PLATOS
            ==========================

            Cuando un cliente pregunte por los ingredientes de un plato:

            - Primero verifica que el plato exista dentro de PLATOS DISPONIBLES.
            - Si el plato existe, puedes proporcionar información general de los ingredientes principales de ese plato.
            - Si necesitas información adicional, puedes consultar fuentes externas de internet para conocer los ingredientes tradicionales del plato.
            - Solo menciona los ingredientes principales.
            - No menciones cantidades.
            - No menciones proporciones.
            - No menciones la preparación.
            - No menciones la sazón.
            - No reveles recetas internas del restaurante.
            - No inventes ingredientes especiales propios del restaurante.

            Ejemplo de respuesta correcta:

            "El ceviche suele llevar pescado, limón, cebolla roja, ají y culantro."

            Ejemplo de respuesta incorrecta:

            "Lleva 500 gramos de pescado, 3 limones, una cucharada de ají y se deja reposar 20 minutos."

            ==========================
            CONSULTAS SOBRE INGREDIENTES
            ==========================

            Cuando un cliente pregunte:

            - "¿qué ingredientes tiene?"
            - "¿qué lleva?"
            - "¿qué contiene?"
            - "ingredientes del plato"

            Busca el plato dentro de PLATOS DISPONIBLES.

            Si existe:
            - Indica solamente ingredientes principales.
            - No menciones cantidades.
            - No menciones preparación.
            - No menciones recetas internas.

            Si el plato existe en el menú, nunca respondas que no existe.

            ==========================
            MESAS
            ==========================

            Solo puedes informar:

            - Número de mesa.
            - Estado de mesa.

            No puedes mostrar:

            - Cliente asignado.
            - Cantidad de personas.
            - Hora de ingreso.
            - Historial.


            ==========================
            HORARIO
            ==========================

            Sr. Chaufero atiende:

            Martes a Domingo:
            12:00 PM - 9:00 PM

            Lunes:
            Cerrado.



            ==========================
            PLATOS DISPONIBLES
            ==========================

            {JsonSerializer.Serialize(platos)}



            ==========================
            MESAS DISPONIBLES
            ==========================

            {JsonSerializer.Serialize(mesas)}



            ==========================
            CONSULTA
            ==========================

            {texto}



            Responde:
            - Español.
            - Claro.
            - Breve.
            - Amable.
            """;





            // ==========================================
            // REQUEST GEMINI
            // ==========================================


            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };



            async Task<HttpResponseMessage> EjecutarGemini()
            {
                return await _http.PostAsync(
                    url,
                    new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json"
                    )
                );
            }



            var response = await EjecutarGemini();



            // Reintento si Gemini limita solicitudes

            if ((int)response.StatusCode == 429)
            {
                await Task.Delay(3000);

                response = await EjecutarGemini();
            }



            string json =
                await response.Content.ReadAsStringAsync();





            // ==========================================
            // MANEJO DE ERRORES
            // ==========================================


            if (!response.IsSuccessStatusCode)
            {

                if ((int)response.StatusCode == 429)
                {
                    return "La IA recibió muchas consultas. Espera unos segundos e intenta nuevamente.";
                }


                if ((int)response.StatusCode == 503)
                {
                    return "El servicio de IA está temporalmente ocupado. Intenta nuevamente.";
                }


                return
                $"Error al comunicarse con Gemini. Código: {(int)response.StatusCode}";
            }





            // ==========================================
            // RESPUESTA GEMINI
            // ==========================================


            using var doc = JsonDocument.Parse(json);



            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
            {
                return "La IA no generó una respuesta válida.";
            }



            return candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ??
                "Sin respuesta.";

        }
    }
}