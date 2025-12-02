using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;
using SistemaComercial.Web.Services;
using System.ComponentModel.DataAnnotations;
using Camionetas.Grpc;

namespace SistemaComercial.Web.Pages.Arriendos;

public class CreateModel : PageModel
{
    private readonly ComercialDbContext _context;
    private readonly MantencionGrpcClient _mantencionGrpc;
    private readonly CamionetasApiClient _camionetasApi;

    public CreateModel(
        ComercialDbContext context,
        MantencionGrpcClient mantencionGrpc,
        CamionetasApiClient camionetasApi)
    {
        _context = context;
        _mantencionGrpc = mantencionGrpc;
        _camionetasApi = camionetasApi;
    }

    [BindProperty]
    public ArriendoInput Input { get; set; } = new();

    public SelectList ClientesSelect { get; set; } = null!;
    public SelectList TiposCamionetaSelect { get; set; } = null!;
    public SelectList CamionetasSelect { get; set; } = null!;   // 👈 NUEVO

    public class ArriendoInput
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public string Patente { get; set; } = string.Empty;

        [Required]
        public string TipoCamioneta { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required]
        public DateTime FechaTermino { get; set; } = DateTime.Today.AddDays(1);
    }

    public async Task OnGetAsync()
    {
        await CargarCombosAsync();
    }

    private async Task CargarCombosAsync()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        var tipos = await _context.PreciosArriendo
            .Select(p => p.TipoCamioneta)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        var camionetas = await _camionetasApi.GetDisponiblesAsync();

        ClientesSelect = new SelectList(clientes, "Id", "Nombre");
        TiposCamionetaSelect = new SelectList(tipos);

        CamionetasSelect = new SelectList(
            camionetas,
            nameof(CamionetasApiClient.CamionetaDto.Patente),
            nameof(CamionetasApiClient.CamionetaDto.Patente)
        );
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCombosAsync();

        if (!ModelState.IsValid)
            return Page();

        var inicioLocal = Input.FechaInicio.Date;
        var terminoLocal = Input.FechaTermino.Date;

        if (terminoLocal < inicioLocal)
        {
            ModelState.AddModelError(string.Empty, "La fecha de término no puede ser anterior a la fecha de inicio.");
            return Page();
        }

        var inicioUtc = DateTime.SpecifyKind(inicioLocal, DateTimeKind.Utc);
        var terminoUtc = DateTime.SpecifyKind(terminoLocal, DateTimeKind.Utc);

        var cliente = await _context.Clientes.FindAsync(Input.ClienteId);
        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, "El cliente seleccionado no existe.");
            return Page();
        }

        CamionetaEstadoResponse estadoCamioneta;
        try
        {
            estadoCamioneta = await _mantencionGrpc.ConsultarCamioneta(Input.Patente);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty,
                $"Error al contactar sistema de mantención: {ex.Message}");
            return Page();
        }


        if (estadoCamioneta.Estado == "NoExiste")
        {
            ModelState.AddModelError(string.Empty, $"La camioneta con patente {Input.Patente} no existe en el sistema de mantención.");
            return Page();
        }

        if (!estadoCamioneta.Disponible)
        {
            var msg = estadoCamioneta.Estado switch
            {
                var s when s == EstadoCamioneta.EnMantencion =>
                    $"La camioneta {Input.Patente} está en mantención y no puede ser arrendada.",
                var s when s == EstadoCamioneta.EnArriendo =>
                    $"La camioneta {Input.Patente} ya está en arriendo.",
                _ => $"La camioneta {Input.Patente} no está disponible para arriendo. Estado actual: {estadoCamioneta.Estado}."
            };

            ModelState.AddModelError(string.Empty, msg);
            return Page();
        }


        var precio = await _context.PreciosArriendo
            .FirstOrDefaultAsync(p => p.TipoCamioneta == Input.TipoCamioneta);

        if (precio is null)
        {
            ModelState.AddModelError(string.Empty, "No existe un precio configurado para ese tipo de camioneta.");
            return Page();
        }

        var dias = (terminoUtc - inicioUtc).Days;
        if (dias <= 0) dias = 1;

        var total = dias * precio.PrecioPorDia;

        var cambio = await _mantencionGrpc.CambiarEstado(Input.Patente, EstadoCamioneta.EnArriendo);
        if (!cambio.Success)
        {
            ModelState.AddModelError(string.Empty, $"No se pudo cambiar el estado de la camioneta en mantención: {cambio.Message}");
            return Page();
        }

        var arriendo = new Arriendo
        {
            ClienteId = Input.ClienteId,
            Patente = Input.Patente,
            FechaInicio = inicioUtc,
            FechaTermino = terminoUtc,
            PrecioTotal = total
        };

        _context.Arriendos.Add(arriendo);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}