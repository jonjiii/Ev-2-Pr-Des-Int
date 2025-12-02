using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Clientes;

public class EditModel : PageModel
{
    private readonly ComercialDbContext _context;

    public EditModel(ComercialDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Cliente Cliente { get; set; } = new Cliente();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
        {
            return NotFound();
        }

        Cliente = cliente;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var clienteDb = await _context.Clientes.FindAsync(Cliente.Id);
        if (clienteDb == null)
        {
            return NotFound();
        }

        clienteDb.Nombre = Cliente.Nombre;
        clienteDb.Rut = Cliente.Rut;
        clienteDb.Telefono = Cliente.Telefono;
        clienteDb.Correo = Cliente.Correo;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToPage("Index");
    }
}