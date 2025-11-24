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
    public FacturaInput Input { get; set; } = new FacturaInput();

    public SelectList ArriendosSelect { get; set; } = null!;
    public decimal TotalArriendo { get; set; }

    public async Task OnGetAsync(int? arriendoId)
    {
        await CargarArriendosAsync();

        if (arriendoId.HasValue)
        {
            var arr = await _context.Arriendos.FindAsync(arriendoId.Value);
            if (arr != null)
            {
                Input.ArriendoId = arr.Id;
                TotalArriendo = arr.PrecioTotal;
            }
        }
    }

    private async Task CargarArriendosAsync()
    {
        var arriendos = await _context.Arriendos
            .Where(a => a.Factura == null) // solo arriendos sin facturar
            .Include(a => a.Cliente)
            .OrderBy(a => a.FechaInicio)
            .ToListAsync();

        ArriendosSelect = new SelectList(
            arriendos.Select(a =>
                new {
                    Id = a.Id,
                    Texto = $"{a.Id} - {a.Cliente.Nombre} ({a.Patente})"
                }),
            "Id",
            "Texto"
        );
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarArriendosAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

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
            ModelState.AddModelError(string.Empty, "Ese arriendo ya está facturado.");
            return Page();
        }

        var factura = new Factura
        {
            ArriendoId = arriendo.Id,
            Monto = arriendo.PrecioTotal,
            FechaEmision = DateTime.Now
        };


        _context.Facturas.Add(factura);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public class FacturaInput
    {
        public int ArriendoId { get; set; }
    }
}
