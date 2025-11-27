using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Models;

namespace SistemaMantencion.Web.Data;

public class MantencionDbContext : DbContext
{
    public MantencionDbContext(DbContextOptions<MantencionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Camioneta> Camionetas { get; set; } = null!;
    public DbSet<Mantencion> Mantenciones { get; set; } = null!;

}
