using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Pages.Clientes;

public class IndexModel : PageModel
{
    private readonly ComercialDbContext _context;

    public IndexModel(ComercialDbContext context)
    {
        _context = context;
    }

    public IList<Cliente> ListaClientes { get; set; } = new List<Cliente>();

    public async Task OnGetAsync()
    {
        ListaClientes = await _context.Clientes.AsNoTracking().ToListAsync();
    }
}
