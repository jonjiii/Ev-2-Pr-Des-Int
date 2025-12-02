using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;
using SistemaComercial.Web.Services;
using System.Text.Json.Serialization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var mantencionUrl = builder.Configuration.GetSection("Grpc")["MantencionUrl"]
                   ?? "https://localhost:7227";

builder.Services.AddSingleton(
    new MantencionGrpcClient(mantencionUrl)
);

builder.Services.AddHttpClient<CamionetasApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5287");
    client.DefaultRequestVersion = HttpVersion.Version11;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
});

// OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddRazorPages();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<ComercialDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ComercialDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapRazorPages();

// ==============================
// ENDPOINTS — CLIENTES
// ==============================
var clientes_group = app.MapGroup("/api/clientes");

clientes_group.MapGet("/", async (ComercialDbContext db) =>
    await db.Clientes.AsNoTracking().ToListAsync());

clientes_group.MapGet("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    return cliente is not null ? Results.Ok(cliente) : Results.NotFound();
});

clientes_group.MapPost("/", async (Cliente cliente, ComercialDbContext db) =>
{
    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();
    return Results.Created($"/api/clientes/{cliente.Id}", cliente);
});

clientes_group.MapPut("/{id:int}", async (int id, Cliente input, ComercialDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    if (cliente is null) return Results.NotFound();

    cliente.Nombre = input.Nombre;
    cliente.Rut = input.Rut;
    cliente.Telefono = input.Telefono;
    cliente.Correo = input.Correo;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

clientes_group.MapDelete("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    if (cliente is null) return Results.NotFound();

    db.Clientes.Remove(cliente);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ==============================
// ENDPOINTS — PRECIOS
// ==============================
var precios_group = app.MapGroup("/api/precios");

precios_group.MapGet("/", async (ComercialDbContext db) =>
    await db.PreciosArriendo.AsNoTracking().ToListAsync());

precios_group.MapGet("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var precio = await db.PreciosArriendo.FindAsync(id);
    return precio is not null ? Results.Ok(precio) : Results.NotFound();
});

precios_group.MapPost("/", async (PrecioArriendo precio, ComercialDbContext db) =>
{
    db.PreciosArriendo.Add(precio);
    await db.SaveChangesAsync();
    return Results.Created($"/api/precios/{precio.Id}", precio);
});

precios_group.MapPut("/{id:int}", async (int id, PrecioArriendo input, ComercialDbContext db) =>
{
    var precio = await db.PreciosArriendo.FindAsync(id);
    if (precio is null) return Results.NotFound();

    precio.TipoCamioneta = input.TipoCamioneta;
    precio.PrecioPorDia = input.PrecioPorDia;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

precios_group.MapDelete("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var precio = await db.PreciosArriendo.FindAsync(id);
    if (precio is null) return Results.NotFound();

    db.PreciosArriendo.Remove(precio);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ==============================
// ENDPOINTS — ARRIENDOS
//   (con gRPC integrado)
// ==============================
var arriendos_group = app.MapGroup("/api/arriendos");

// GET /api/arriendos
arriendos_group.MapGet("/", async (ComercialDbContext db) =>
    await db.Arriendos
        .Include(a => a.Cliente)
        .Include(a => a.PrecioArriendo)
        .AsNoTracking()
        .ToListAsync());

// GET /api/arriendos/{id}
arriendos_group.MapGet("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var arriendo = await db.Arriendos
        .Include(a => a.Cliente)
        .Include(a => a.PrecioArriendo)
        .Include(a => a.Factura)
        .FirstOrDefaultAsync(a => a.Id == id);

    return arriendo is not null ? Results.Ok(arriendo) : Results.NotFound();
});

// POST /api/arriendos/finalizar/{id}
arriendos_group.MapPost("/finalizar/{id:int}", async (int id, ComercialDbContext db, MantencionGrpcClient grpc) =>
{
    var arriendo = await db.Arriendos.FindAsync(id);
    if (arriendo is null)
        return Results.NotFound();

    // Cambiar en mantención
    var cambio = await grpc.CambiarEstado(arriendo.Patente, EstadoCamioneta.Disponible);
    if (!cambio.Success)
        return Results.BadRequest(cambio.Message);

    // Marcar en BD como completado
    arriendo.FechaTermino = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok();
});

// POST /api/arriendos  → crear arriendo nuevo
arriendos_group.MapPost("/", async (
    CrearArriendoRequest request,
    ComercialDbContext db,
    MantencionGrpcClient mantencionGrpc) =>
{
    // 1) Validar cliente
    var cliente = await db.Clientes.FindAsync(request.ClienteId);
    if (cliente is null)
        return Results.BadRequest($"No existe cliente con Id {request.ClienteId}");

    // 2) Consultar camioneta en SISTEMA DE MANTENCIÓN (gRPC)
    var estadoCamioneta = await mantencionGrpc.ConsultarCamioneta(request.Patente);

    if (estadoCamioneta.Estado == "NoExiste")
        return Results.BadRequest($"La camioneta {request.Patente} no existe en Mantención.");

    if (!estadoCamioneta.Disponible)
        return Results.BadRequest($"La camioneta {request.Patente} está en estado {estadoCamioneta.Estado}.");

    // 3) Cambiar estado a EnArriendo en Mantención (gRPC)
    var cambio = await mantencionGrpc.CambiarEstado(request.Patente, EstadoCamioneta.EnArriendo);
    if (!cambio.Success)
        return Results.BadRequest($"No se pudo cambiar estado: {cambio.Message}");

    // 4) Obtener precio desde tabla PrecioArriendo (FK)
    var precio = await db.PreciosArriendo.FindAsync(request.PrecioArriendoId);
    if (precio is null)
        return Results.BadRequest($"No existe precio con Id {request.PrecioArriendoId}");

    // 5) Calcular total
    var inicio = request.FechaInicio.Date;
    var termino = request.FechaTermino.Date;
    var dias = (termino - inicio).Days;
    if (dias <= 0) dias = 1;

    var total = dias * precio.PrecioPorDia;

    // 6) Crear arriendo
    var arriendo = new Arriendo
    {
        ClienteId = request.ClienteId,
        Patente = request.Patente,
        FechaInicio = inicio,
        FechaTermino = termino,
        PrecioArriendoId = precio.Id,
        PrecioTotal = total
    };

    db.Arriendos.Add(arriendo);
    await db.SaveChangesAsync();

    return Results.Created($"/api/arriendos/{arriendo.Id}", arriendo);
});

// ==============================
// ENDPOINTS — FACTURAS
// ==============================
var facturas_group = app.MapGroup("/api/facturas");

facturas_group.MapGet("/", async (ComercialDbContext db) =>
    await db.Facturas
        .Include(f => f.Arriendo)
        .ThenInclude(a => a.Cliente)
        .ToListAsync());

facturas_group.MapGet("/{id:int}", async (int id, ComercialDbContext db) =>
{
    var factura = await db.Facturas
        .Include(f => f.Arriendo)
        .ThenInclude(a => a.Cliente)
        .FirstOrDefaultAsync(f => f.Id == id);

    return factura is not null ? Results.Ok(factura) : Results.NotFound();
});

facturas_group.MapPost("/", async (CrearFacturaRequest request, ComercialDbContext db) =>
{
    var arriendo = await db.Arriendos
        .Include(a => a.Factura)
        .FirstOrDefaultAsync(a => a.Id == request.ArriendoId);

    if (arriendo is null)
        return Results.BadRequest($"No existe arriendo {request.ArriendoId}");

    if (arriendo.Factura is not null)
        return Results.Conflict("Ya tiene factura");

    var factura = new Factura
    {
        ArriendoId = arriendo.Id,
        FechaEmision = DateTime.UtcNow,
        Monto = arriendo.PrecioTotal
    };

    db.Facturas.Add(factura);
    await db.SaveChangesAsync();

    return Results.Created($"/api/facturas/{factura.Id}", factura);
});

var summaries = new[]
{
    "Freezing","Bracing","Chilly","Cool","Mild","Warm","Balmy","Hot","Sweltering","Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )
    ).ToArray();

    return forecast;
});

app.Run();

// ======= RECORD TYPES =======
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Ahora el request usa PrecioArriendoId en vez de TipoCamioneta
record CrearArriendoRequest(
    int ClienteId,
    string Patente,
    DateTime FechaInicio,
    DateTime FechaTermino,
    int PrecioArriendoId
);

record CrearFacturaRequest(int ArriendoId);
