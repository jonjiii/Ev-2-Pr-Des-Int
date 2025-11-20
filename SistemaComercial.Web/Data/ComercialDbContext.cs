using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Models;

namespace SistemaComercial.Web.Data;

public class ComercialDbContext : DbContext
{
    public ComercialDbContext(DbContextOptions<ComercialDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Arriendo> Arriendos => Set<Arriendo>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<PrecioArriendo> PreciosArriendo => Set<PrecioArriendo>();
}
