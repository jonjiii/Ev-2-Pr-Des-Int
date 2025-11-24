using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Precios;

public class EditModel : PageModel
{
    private readonly ComercialDbContext _context;

    public EditModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PrecioArriendo Precio { get; set; } = new PrecioArriendo();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var precio = await _context.PreciosArriendo.FindAsync(id);

        if (precio == null)
        {
            return NotFound();
        }

        Precio = precio;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(Precio).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.PreciosArriendo.Any(p => p.Id == Precio.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("Index");
    }
}
