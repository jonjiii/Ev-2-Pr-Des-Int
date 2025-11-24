using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Precios;

public class DeleteModel : PageModel
{
    private readonly ComercialDbContext _context;

    public DeleteModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PrecioArriendo Precio { get; set; } = new PrecioArriendo();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var precio = await _context.PreciosArriendo
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (precio == null)
        {
            return NotFound();
        }

        Precio = precio;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var precio = await _context.PreciosArriendo.FindAsync(id);

        if (precio == null)
        {
            return NotFound();
        }

        _context.PreciosArriendo.Remove(precio);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
