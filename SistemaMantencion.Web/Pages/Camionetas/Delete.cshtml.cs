using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class DeleteModel : PageModel
{
    private readonly MantencionDbContext _context;

    public DeleteModel(MantencionDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Camioneta Camioneta { get; set; } = new Camioneta();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cam = await _context.Camionetas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cam == null)
        {
            return NotFound();
        }

        Camioneta = cam;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var cam = await _context.Camionetas.FindAsync(id);

        if (cam == null)
        {
            return NotFound();
        }

        _context.Camionetas.Remove(cam);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
