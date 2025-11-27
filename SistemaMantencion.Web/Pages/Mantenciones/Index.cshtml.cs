using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Mantenciones;

public class IndexModel : PageModel
{
    private readonly MantencionDbContext _context;

    public IndexModel(MantencionDbContext context)
    {
        _context = context;
    }

    public IList<Mantencion> Lista { get; set; } = new List<Mantencion>();

    public async Task OnGetAsync()
    {
        Lista = await _context.Mantenciones
            .Include(m => m.Camioneta)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }
}
