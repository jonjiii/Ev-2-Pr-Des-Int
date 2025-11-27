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

app.UseHttpsRedirection();

app.MapRazorPages();

app.MapGet("/", () => "Sistema de Mantención OK");

// ENDPOINTS HTTP — CAMIONETAS
var camionetas_group = app.MapGroup("/api/camionetas");

// GET todas
camionetas_group.MapGet("/", async (MantencionDbContext db) =>
    await db.Camionetas.AsNoTracking().ToListAsync());

// GET por id
camionetas_group.MapGet("/{id:int}", async (int id, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    return cam is not null ? Results.Ok(cam) : Results.NotFound();
});

// POST crear
camionetas_group.MapPost("/", async (Camioneta cam, MantencionDbContext db) =>
{
    db.Camionetas.Add(cam);
    await db.SaveChangesAsync();
    return Results.Created($"/api/camionetas/{cam.Id}", cam);
});

// PUT actualizar (opcional)
camionetas_group.MapPut("/{id:int}", async (int id, Camioneta input, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    if (cam is null) return Results.NotFound();

    cam.Patente = input.Patente;
    cam.Estado = input.Estado;
    cam.Kilometraje = input.Kilometraje;
    cam.DisponibleParaArriendo = input.DisponibleParaArriendo;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE (opcional)
camionetas_group.MapDelete("/{id:int}", async (int id, MantencionDbContext db) =>
{
    var cam = await db.Camionetas.FindAsync(id);
    if (cam is null) return Results.NotFound();

    db.Camionetas.Remove(cam);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();


