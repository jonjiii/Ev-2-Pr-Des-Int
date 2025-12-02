using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;
using SistemaComercial.Web.Services;

using Camionetas.Grpc;

namespace SistemaComercial.Web.Pages.Arriendos;

public class IndexModel : PageModel
{
    private readonly ComercialDbContext _context;
    private readonly MantencionGrpcClient _mantencionGrpc;

    public IndexModel(ComercialDbContext context, MantencionGrpcClient mantencionGrpc)
    {
        _context = context;
        _mantencionGrpc = mantencionGrpc;
    }

    public IList<Arriendo> Lista { get; set; } = new List<Arriendo>();

    public async Task OnGetAsync()
    {
        Lista = await _context.Arriendos
            .Include(a => a.Cliente)
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostFinalizarAsync(int id)
    {
        var arriendo = await _context.Arriendos.FindAsync(id);
        if (arriendo is null)
            return NotFound();

        try
        {
            var cambio = await _mantencionGrpc.CambiarEstado(arriendo.Patente, EstadoCamioneta.Disponible);
            if (!cambio.Success)
            {
                TempData["Error"] = $"No se pudo cambiar el estado de la camioneta: {cambio.Message}";
                return RedirectToPage();
            }
        }
        catch
        {
            TempData["Error"] = "No se pudo contactar al sistema de mantención.";
            return RedirectToPage();
        }

        arriendo.FechaTermino = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Arriendo finalizado correctamente.";
        return RedirectToPage();
    }
}

