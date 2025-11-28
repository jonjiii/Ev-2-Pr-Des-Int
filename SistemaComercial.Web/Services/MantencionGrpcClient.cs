using Camionetas.Grpc;
using Grpc.Net.Client;

namespace SistemaComercial.Web.Services;

public class MantencionGrpcClient
{
    private readonly MantencionService.MantencionServiceClient _client;

    public MantencionGrpcClient(string baseAddress)
    {
        var channel = GrpcChannel.ForAddress(baseAddress);
        _client = new MantencionService.MantencionServiceClient(channel);
    }


    public Task<CamionetaEstadoResponse> ConsultarCamioneta(string patente)
    {
        return _client.ConsultarCamionetaAsync(new CamionetaRequest
        {
            Patente = patente
        }).ResponseAsync;
    }

    public Task<CambiarEstadoResponse> CambiarEstado(string patente, string nuevoEstado)
    {
        return _client.CambiarEstadoAsync(new CambiarEstadoRequest
        {
            Patente = patente,
            NuevoEstado = nuevoEstado
        }).ResponseAsync;
    }
}
