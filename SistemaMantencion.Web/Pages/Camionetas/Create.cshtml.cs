using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Pages.Camionetas;

public class CreateModel : PageModel
{
    private readonly MantencionDbContext _context;

    public CreateModel(MantencionDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Camioneta Camioneta { get; set; } = new Camioneta();

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Camionetas.Add(Camioneta);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
