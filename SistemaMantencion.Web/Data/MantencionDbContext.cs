using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Data;

public class MantencionDbContext : DbContext
{
    public MantencionDbContext(DbContextOptions<MantencionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Camioneta> Camionetas => Set<Camioneta>();
    public DbSet<Mantencion> Mantenciones => Set<Mantencion>();
}
