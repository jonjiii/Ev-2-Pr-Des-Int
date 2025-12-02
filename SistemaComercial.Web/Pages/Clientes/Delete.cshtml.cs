using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Clientes;

public class DeleteModel : PageModel
{
    private readonly ComercialDbContext _context;

    public DeleteModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Cliente Cliente { get; set; } = new Cliente();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            return NotFound();
        }

        Cliente = cliente;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Cliente eliminado correctamente.";
        return RedirectToPage("Index");
    }
}