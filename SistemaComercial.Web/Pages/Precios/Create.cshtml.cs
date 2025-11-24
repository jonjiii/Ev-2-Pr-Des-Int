using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Precios;

public class CreateModel : PageModel
{
    private readonly ComercialDbContext _context;

    public CreateModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PrecioArriendo NuevoPrecio { get; set; } = new PrecioArriendo();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.PreciosArriendo.Add(NuevoPrecio);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
