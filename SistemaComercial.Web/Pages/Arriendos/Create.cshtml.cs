using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaComercial.Web.Data;
using SistemaComercial.Web.Models;
using SistemaComercial.Web.Services;

namespace SistemaComercial.Web.Pages.Arriendos;

public class CreateModel : PageModel
{
    private readonly ComercialDbContext _context;
    private readonly MantencionGrpcClient _mantencionGrpc;

    public CreateModel(ComercialDbContext context, MantencionGrpcClient mantencionGrpc)
    {
        _context = context;
        _mantencionGrpc = mantencionGrpc;
    }

    [BindProperty]
    public ArriendoInput Input { get; set; } = new ArriendoInput();

    public SelectList ClientesSelect { get; set; } = null!;
    public SelectList TiposCamionetaSelect { get; set; } = null!;

    public async Task OnGetAsync()
    {
        await CargarCombosAsync();
        // valores por defecto
        Input.FechaInicio = DateTime.Today;
        Input.FechaTermino = DateTime.Today.AddDays(1);
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

        ClientesSelect = new SelectList(clientes, "Id", "Nombre");
        TiposCamionetaSelect = new SelectList(tipos);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCombosAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1) Validar cliente
        var cliente = await _context.Clientes.FindAsync(Input.ClienteId);
        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, $"No existe cliente con Id {Input.ClienteId}");
            return Page();
        }

        // 2) Validar camioneta en SISTEMA DE MANTENCIÓN (gRPC)
        var estadoCamioneta = await _mantencionGrpc.ConsultarCamioneta(Input.Patente);

        if (estadoCamioneta.Estado == "NoExiste")
        {
            ModelState.AddModelError(string.Empty, $"La camioneta {Input.Patente} no existe en Mantención.");
            return Page();
        }

        if (!estadoCamioneta.Disponible)
        {
            ModelState.AddModelError(string.Empty,
                $"La camioneta {Input.Patente} está en estado {estadoCamioneta.Estado} y no está disponible para arriendo.");
            return Page();
        }

        // 3) Cambiar estado a En Arriendo en Mantención (gRPC)
        var cambio = await _mantencionGrpc.CambiarEstado(Input.Patente, "En Arriendo");
        if (!cambio.Success)
        {
            ModelState.AddModelError(string.Empty, $"No se pudo cambiar estado en Mantención: {cambio.Message}");
            return Page();
        }

        // 4) Obtener precio según tipo de camioneta
        var precio = await _context.PreciosArriendo
            .FirstOrDefaultAsync(p => p.TipoCamioneta == Input.TipoCamioneta);

        if (precio is null)
        {
            ModelState.AddModelError(string.Empty, "No existe precio configurado para ese tipo de camioneta.");
            return Page();
        }

        // 5) Calcular días y total
        var inicio = Input.FechaInicio.Date;
        var termino = Input.FechaTermino.Date;
        var dias = (termino - inicio).Days;
        if (dias <= 0) dias = 1;

        var total = dias * precio.PrecioPorDia;

        // 6) Crear arriendo
        var arriendo = new Arriendo
        {
            ClienteId = Input.ClienteId,
            Patente = Input.Patente,
            FechaInicio = inicio,
            FechaTermino = termino,
            PrecioTotal = total
        };

        _context.Arriendos.Add(arriendo);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    // ViewModel para el formulario
    public class ArriendoInput
    {
        public int ClienteId { get; set; }
        public string Patente { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public string TipoCamioneta { get; set; } = string.Empty;
    }
}
