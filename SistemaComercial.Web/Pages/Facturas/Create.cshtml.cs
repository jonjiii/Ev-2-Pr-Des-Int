using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Facturas;

public class CreateModel : PageModel
{
    private readonly ComercialDbContext _context;

    public CreateModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public FacturaInput Input { get; set; } = new();

    public SelectList ArriendosSelect { get; set; } = null!;

    // NUEVO → Total previo que se mostrará en la vista
    public decimal? TotalPrevio { get; set; }

    public class FacturaInput
    {
        public int ArriendoId { get; set; }
    }

    public async Task OnGetAsync(int? arriendoId)
    {
        await CargarArriendosAsync();

        if (arriendoId.HasValue)
        {
            Input.ArriendoId = arriendoId.Value;

            var arriendo = await _context.Arriendos
                .FirstOrDefaultAsync(a => a.Id == arriendoId.Value);

            if (arriendo != null)
            {
                TotalPrevio = arriendo.PrecioTotal;
            }
        }
    }

    private async Task CargarArriendosAsync()
    {
        var arriendos = await _context.Arriendos
            .Where(a => a.Factura == null && a.FechaTermino != null)    // solo finalizados y sin factura
            .Include(a => a.Cliente)
            .OrderBy(a => a.FechaInicio)
            .ToListAsync();

        ArriendosSelect = new SelectList(
            arriendos.Select(a => new
            {
                a.Id,
                Texto = $"{a.Id} — {a.Cliente.Nombre} ({a.Patente})"
            }),
            "Id",
            "Texto"
        );
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarArriendosAsync();

        if (!ModelState.IsValid)
            return Page();

        var arriendo = await _context.Arriendos
            .Include(a => a.Factura)
            .FirstOrDefaultAsync(a => a.Id == Input.ArriendoId);

        if (arriendo == null)
        {
            ModelState.AddModelError(string.Empty, "El arriendo seleccionado no existe.");
            return Page();
        }

        if (arriendo.Factura != null)
        {
            ModelState.AddModelError(string.Empty, "Ese arriendo ya tiene factura emitida.");
            return Page();
        }

        if (arriendo.FechaTermino == null)
        {
            ModelState.AddModelError(string.Empty, "No se puede facturar un arriendo que aún no está finalizado.");
            return Page();
        }

        var factura = new Factura
        {
            ArriendoId = arriendo.Id,
            Monto = arriendo.PrecioTotal,
            FechaEmision = DateTime.UtcNow
        };

        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}

