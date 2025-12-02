using Camionetas.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Models;
using SistemaMantencion.Web.Data;

namespace SistemaMantencion.Web.Services;

public class MantencionGrpcService : MantencionService.MantencionServiceBase
{
    private readonly MantencionDbContext _db;

    public MantencionGrpcService(MantencionDbContext db)
    {
        _db = db;
    }

    public override async Task<CamionetaEstadoResponse> ConsultarCamioneta(
        CamionetaRequest request,
        ServerCallContext context)
    {
        var cam = await _db.Camionetas.FirstOrDefaultAsync(c => c.Patente == request.Patente);

        if (cam == null)
        {
            return new CamionetaEstadoResponse
            {
                Patente = request.Patente,
                Estado = "NoExiste",
                Kilometraje = 0,
                Disponible = false
            };
        }

        return new CamionetaEstadoResponse
        {
            Patente = cam.Patente,
            Estado = cam.Estado,
            Kilometraje = cam.Kilometraje,
            Disponible = cam.DisponibleParaArriendo
        };
    }

    public override async Task<CambiarEstadoResponse> CambiarEstado(
        CambiarEstadoRequest request,
        ServerCallContext context)
    {
        // Propiedad correcta: request.Patente
        var cam = await _db.Camionetas.FirstOrDefaultAsync(c => c.Patente == request.Patente);

        if (cam == null)
        {
            return new CambiarEstadoResponse
            {
                Success = false,
                Message = "Camioneta no encontrada"
            };
        }

        // Propiedad correcta: request.NuevoEstado
        cam.Estado = request.NuevoEstado;
        cam.DisponibleParaArriendo = request.NuevoEstado == EstadoCamioneta.Disponible;

        await _db.SaveChangesAsync();

        return new CambiarEstadoResponse
        {
            Success = true,
            Message = "Estado actualizado correctamente"
        };
    }

    public override async Task<CamionetaEstadoResponse> ObtenerKilometraje(
        CamionetaRequest request,
        ServerCallContext context)
    {
        var cam = await _db.Camionetas.FirstOrDefaultAsync(c => c.Patente == request.Patente);

        if (cam == null)
        {
            return new CamionetaEstadoResponse
            {
                Patente = request.Patente,
                Estado = "NoExiste",
                Kilometraje = 0,
                Disponible = false
            };
        }

        return new CamionetaEstadoResponse
        {
            Patente = cam.Patente,
            Estado = cam.Estado,
            Kilometraje = cam.Kilometraje,
            Disponible = cam.DisponibleParaArriendo
        };
    }
}

