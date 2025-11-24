using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Precios;

public class IndexModel : PageModel
{
    private readonly ComercialDbContext _context;

    public IndexModel(ComercialDbContext context)
    {
        _context = context;
    }

    public IList<PrecioArriendo> ListaPrecios { get; set; } = new List<PrecioArriendo>();

    public async Task OnGetAsync()
    {
        ListaPrecios = await _context.PreciosArriendo
            .AsNoTracking()
            .ToListAsync();
    }
}
