namespace SrChauferoMVC_AzureIA.Services
{
    // ==========================================
    // CONTRATO DEL SERVICIO DE IA
    // ==========================================
    public interface IIAService
    {
        Task<string> RecomendarAsync(string texto);
    }
}