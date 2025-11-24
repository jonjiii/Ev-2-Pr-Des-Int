using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Arriendos;

public class IndexModel : PageModel
{
    private readonly ComercialDbContext _context;

    public IndexModel(ComercialDbContext context)
    {
        _context = context;
    }

    public IList<Arriendo> ListaArriendos { get; set; } = new List<Arriendo>();

    public async Task OnGetAsync()
    {
        ListaArriendos = await _context.Arriendos
            .Include(a => a.Cliente)
            .Include(a => a.Factura)
            .AsNoTracking()
            .ToListAsync();
    }
}
