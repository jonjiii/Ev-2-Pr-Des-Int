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

// ===============================
//   ENDPOINTS — CAMIONETAS (API)
// ===============================
var camionetas_group = app.MapGroup("/api/camionetas");

// GET /api/camionetas  → listado simple para otros servicios
camionetas_group.MapGet("/", async (MantencionDbContext db) =>
    await db.Camionetas.AsNoTracking()
        .Select(c => new
        {
            c.Id,
            c.Patente,
            c.Kilometraje,
            c.Estado,
            c.DisponibleParaArriendo
        })
        .ToListAsync());

// GET /api/camionetas/{id}  → obtener por ID
camionetas_group.MapGet("/{id:int}", async (int id, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);

    return cam is not null ? Results.Ok(cam) : Results.NotFound();
});

// GET /api/camionetas/patente/{patente}  → obtener por patente (ya lo usas)
camionetas_group.MapGet("/patente/{patente}", async (string patente, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FirstOrDefaultAsync(c => c.Patente == patente);
    return cam is not null ? Results.Ok(cam) : Results.NotFound();
});

// POST /api/camionetas  → crear camioneta
camionetas_group.MapPost("/", async (CamionetaCreateDto dto, MantencionDbContext db) =>
{
    var existente = await db.Camionetas.AnyAsync(c => c.Patente == dto.Patente);
    if (existente)
        return Results.Conflict($"Ya existe una camioneta con patente {dto.Patente}");

    var cam = new Camioneta
    {
        Patente = dto.Patente,
        Kilometraje = dto.Kilometraje,
        Estado = EstadoCamioneta.Disponible,
        DisponibleParaArriendo = true
    };

    db.Camionetas.Add(cam);
    await db.SaveChangesAsync();

    return Results.Created($"/api/camionetas/{cam.Id}", cam);
});

// PUT /api/camionetas/{id}  → actualizar camioneta completa
camionetas_group.MapPut("/{id:int}", async (int id, CamionetaUpdateDto dto, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    if (cam is null)
        return Results.NotFound();

    cam.Patente = dto.Patente;
    cam.Kilometraje = dto.Kilometraje;
    cam.Estado = dto.Estado;
    cam.DisponibleParaArriendo = dto.DisponibleParaArriendo;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// PATCH /api/camionetas/{id}/estado  → cambiar solo el estado
camionetas_group.MapPatch("/{id:int}/estado", async (int id, CambiarEstadoDto dto, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    if (cam is null)
        return Results.NotFound();

    cam.Estado = dto.Estado;
    cam.DisponibleParaArriendo = dto.Estado == EstadoCamioneta.Disponible;

    await db.SaveChangesAsync();
    return Results.Ok(cam);
});

// DELETE /api/camionetas/{id}  → eliminar
camionetas_group.MapDelete("/{id:int}", async (int id, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    if (cam is null)
        return Results.NotFound();

    // Nota: si quisieras ser más estricto, aquí podrías validar que no tenga mantenciones asociadas.
    db.Camionetas.Remove(cam);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// ======================
//   DTOs para la API
// ======================

public record CamionetaCreateDto(string Patente, int Kilometraje);

public record CamionetaUpdateDto(
    string Patente,
    int Kilometraje,
    string Estado,
    bool DisponibleParaArriendo
);

public record CambiarEstadoDto(string Estado);
