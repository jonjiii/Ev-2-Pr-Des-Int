using System.Net.Http.Json;

namespace SistemaComercial.Web.Services;

public class CamionetasApiClient
{
    private readonly HttpClient _http;

    public CamionetasApiClient(HttpClient http)
    {
        _http = http;
    }

    public class CamionetaDto
    {
        public int Id { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool DisponibleParaArriendo { get; set; }
    }

    public async Task<List<CamionetaDto>> GetDisponiblesAsync()
    {
        var lista = await _http.GetFromJsonAsync<List<CamionetaDto>>("/api/camionetas");
        if (lista is null) return new();

        // Solo las disponibles
        return lista
            .Where(c => c.DisponibleParaArriendo && c.Estado == "Disponible")
            .ToList();
    }
}
