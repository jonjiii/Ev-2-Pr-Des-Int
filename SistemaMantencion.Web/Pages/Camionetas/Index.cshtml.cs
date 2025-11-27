using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class IndexModel : PageModel
{
    private readonly MantencionDbContext _context;

    public IndexModel(MantencionDbContext context)
    {
        _context = context;
    }

    public IList<Camioneta> Lista { get; set; } = new List<Camioneta>();

    public async Task OnGetAsync()
    {
        Lista = await _context.Camionetas
            .AsNoTracking()
            .ToListAsync();
    }
}
