using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaMantencion.Web.Data;
using SistemaMantencion.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaMantencion.Web.Pages.Mantenciones;

public class CreateModel : PageModel
{
    private readonly MantencionDbContext _context;

    public CreateModel(MantencionDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public MantencionInput Input { get; set; } = new();

    public Camioneta Camioneta { get; set; } = null!;

    public class MantencionInput
    {
        public int CamionetaId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow.Date;
        public string Tipo { get; set; } = "Preventiva";
        public int Kilometraje { get; set; }
        public string Detalle { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync(int camionetaId)
    {
        var cam = await _context.Camionetas.FindAsync(camionetaId);
        if (cam == null)
            return NotFound();

        Camioneta = cam;
        Input.CamionetaId = camionetaId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Normalizamos la fecha ANTES de validar
        Input.Fecha = DateTime.SpecifyKind(Input.Fecha, DateTimeKind.Utc);

        if (!ModelState.IsValid)
        {
            // Recargar la camioneta para volver a mostrar la vista correctamente
            Camioneta = await _context.Camionetas.FindAsync(Input.CamionetaId);
            return Page();
        }

        var cam = await _context.Camionetas.FindAsync(Input.CamionetaId);
        if (cam == null)
            return NotFound();

        var mant = new Mantencion
        {
            CamionetaId = Input.CamionetaId,
            Fecha = Input.Fecha,
            Tipo = Input.Tipo,
            Kilometraje = Input.Kilometraje,
            Detalle = Input.Detalle
        };

        _context.Mantenciones.Add(mant);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Camionetas/Details", new { id = Input.CamionetaId });
    }
}
