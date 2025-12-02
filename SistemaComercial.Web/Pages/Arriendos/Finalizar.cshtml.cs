using Camionetas.Grpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;
using SistemaComercial.Web.Services;

namespace SistemaComercial.Web.Pages.Arriendos;

public class FinalizarModel : PageModel
{
    private readonly ComercialDbContext _context;
    private readonly MantencionGrpcClient _mantencionGrpc;

    public FinalizarModel(
        ComercialDbContext context,
        MantencionGrpcClient mantencionGrpc)
    {
        _context = context;
        _mantencionGrpc = mantencionGrpc;
    }

    [BindProperty]
    public int Id { get; set; }

    public Arriendo? Arriendo { get; set; }

    // Datos calculados para mostrar en la vista
    public DateTime FechaTerminoCalculada { get; set; }
    public int DiasCobrados { get; set; }
    public decimal PrecioPorDia { get; set; }
    public decimal TotalCalculado { get; set; }

    public bool EsFinalizado => Arriendo?.FechaTermino.HasValue == true;
    public bool TieneFactura => Arriendo?.Factura is not null;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;

        Arriendo = await _context.Arriendos
            .Include(a => a.Cliente)
            .Include(a => a.PrecioArriendo)
            .Include(a => a.Factura)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (Arriendo is null)
            return NotFound();

        CalcularResumen();

        return Page();
    }

    // POST por defecto → FINALIZAR ARRIENDO
    public async Task<IActionResult> OnPostAsync()
    {
        Arriendo = await _context.Arriendos
            .Include(a => a.PrecioArriendo)
            .FirstOrDefaultAsync(a => a.Id == Id);

        if (Arriendo is null)
            return NotFound();

        // Si ya está finalizado, no hacemos nada más
        if (Arriendo.FechaTermino.HasValue)
            return RedirectToPage("Index");

        // 1) Calcular fechas y total
        var hoy = DateTime.UtcNow.Date;
        var inicio = Arriendo.FechaInicio.Date;

        var dias = (hoy - inicio).Days;
        if (dias <= 0) dias = 1;

        var precioPorDia = Arriendo.PrecioArriendo.PrecioPorDia;
        var total = dias * precioPorDia;

        // 2) Cambiar estado de la camioneta en Sistema de Mantención (gRPC)
        var cambio = await _mantencionGrpc.CambiarEstado(
            Arriendo.Patente,
            EstadoCamioneta.Disponible
        );

        if (!cambio.Success)
        {
            ModelState.AddModelError(string.Empty,
                $"No se pudo cambiar el estado de la camioneta en el sistema de mantención: {cambio.Message}");

            // Recalcular valores para mostrar de nuevo la página
            CalcularResumen();
            return Page();
        }

        // 3) Actualizar arriendo en la base de datos comercial
        Arriendo.FechaTermino = hoy;
        Arriendo.PrecioTotal = total;

        await _context.SaveChangesAsync();

        return RedirectToPage(new { id = Arriendo.Id });
    }

    // POST handler "Facturar" → GENERAR FACTURA PARA EL ARRIENDO
    public async Task<IActionResult> OnPostFacturarAsync()
    {
        Arriendo = await _context.Arriendos
            .Include(a => a.Factura)
            .FirstOrDefaultAsync(a => a.Id == Id);

        if (Arriendo is null)
            return NotFound();

        if (!Arriendo.FechaTermino.HasValue)
        {
            ModelState.AddModelError(string.Empty,
                "No se puede generar factura para un arriendo que aún no está finalizado.");
            CalcularResumen();
            return Page();
        }

        if (Arriendo.Factura is not null)
        {
            // Ya tiene factura → ir directo a listado
            return RedirectToPage("/Facturas/Index");
        }

        var factura = new Factura
        {
            ArriendoId = Arriendo.Id,
            FechaEmision = DateTime.UtcNow,
            Monto = Arriendo.PrecioTotal
        };

        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Facturas/Index");
    }

    private void CalcularResumen()
    {
        if (Arriendo is null)
            return;

        var hoy = DateTime.UtcNow.Date;

        if (Arriendo.FechaTermino.HasValue)
        {
            FechaTerminoCalculada = Arriendo.FechaTermino.Value.Date;
        }
        else
        {
            FechaTerminoCalculada = hoy;
        }

        var inicio = Arriendo.FechaInicio.Date;
        DiasCobrados = (FechaTerminoCalculada - inicio).Days;
        if (DiasCobrados <= 0) DiasCobrados = 1;

        PrecioPorDia = Arriendo.PrecioArriendo.PrecioPorDia;
        TotalCalculado = DiasCobrados * PrecioPorDia;
    }
}