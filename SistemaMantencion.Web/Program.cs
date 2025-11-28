using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;
using SistemaMantencion.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// Razor Pages
builder.Services.AddRazorPages();

// DbContext Mantención
builder.Services.AddDbContext<MantencionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// Crear BD si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MantencionDbContext>();
    db.Database.EnsureCreated();
}

// Mapear servicio gRPC
app.MapGrpcService<MantencionGrpcService>();

// OpenAPI (solo dev)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapRazorPages();

app.MapGet("/", () => "Sistema de Mantención OK");

// ENDPOINTS — CAMIONETAS
var camionetas_group = app.MapGroup("/api/camionetas");

camionetas_group.MapGet("/", async (MantencionDbContext db) =>
    await db.Camionetas.AsNoTracking()
        .Select(c => new { c.Id, c.Patente, c.Estado, c.DisponibleParaArriendo })
        .ToListAsync());

camionetas_group.MapGet("/patente/{patente}", async (string patente, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FirstOrDefaultAsync(c => c.Patente == patente);
    return cam is not null ? Results.Ok(cam) : Results.NotFound();
});

app.Run();