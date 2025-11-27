using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class EditModel : PageModel
{
    private readonly MantencionDbContext _context;

    public EditModel(MantencionDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Camioneta Camioneta { get; set; } = new Camioneta();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cam = await _context.Camionetas.FindAsync(id);
        if (cam == null)
        {
            return NotFound();
        }

        Camioneta = cam;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var cam = await _context.Camionetas.FindAsync(Camioneta.Id);
        if (cam == null)
            return NotFound();

        cam.Patente = Camioneta.Patente;
        cam.Kilometraje = Camioneta.Kilometraje;
        cam.Estado = Camioneta.Estado;
        cam.DisponibleParaArriendo = Camioneta.DisponibleParaArriendo;

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
