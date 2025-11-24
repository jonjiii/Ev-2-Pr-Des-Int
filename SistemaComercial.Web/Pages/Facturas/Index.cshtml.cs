using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Facturas;

public class IndexModel : PageModel
{
    private readonly ComercialDbContext _context;

    public IndexModel(ComercialDbContext context)
    {
        _context = context;
    }

    public IList<Factura> ListaFacturas { get; set; } = new List<Factura>();

    public async Task OnGetAsync()
    {
        ListaFacturas = await _context.Facturas
            .Include(f => f.Arriendo)
            .ThenInclude(a => a.Cliente)
            .AsNoTracking()
            .ToListAsync();
    }
}
