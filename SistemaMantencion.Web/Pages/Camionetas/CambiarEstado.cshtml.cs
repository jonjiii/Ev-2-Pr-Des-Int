using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class CambiarEstadoModel : PageModel
{
    private readonly MantencionDbContext _context;

    public CambiarEstadoModel(MantencionDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string NuevoEstado { get; set; } = "";

    public Camioneta Camioneta { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, string nuevoEstado)
    {
        var cam = await _context.Camionetas.FindAsync(id);
        if (cam == null)
            return NotFound();

        Camioneta = cam;
        Id = id;
        NuevoEstado = nuevoEstado;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cam = await _context.Camionetas.FindAsync(Id);
        if (cam == null)
            return NotFound();

        // Cambiar estado
        cam.Estado = NuevoEstado;

        // Lógica automática
        cam.DisponibleParaArriendo = NuevoEstado == "Disponible";

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
