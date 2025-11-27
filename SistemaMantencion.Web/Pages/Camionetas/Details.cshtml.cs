using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class DetailsModel : PageModel
{
    private readonly MantencionDbContext _context;

    public DetailsModel(MantencionDbContext context)
    {
        _context = context;
    }

    public Camioneta Camioneta { get; set; } = null!;
    public IList<Mantencion> Mantenciones { get; set; } = new List<Mantencion>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cam = await _context.Camionetas
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cam == null)
            return NotFound();

        Camioneta = cam;

        Mantenciones = await _context.Mantenciones
            .Where(m => m.CamionetaId == id)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Page();
    }
}
