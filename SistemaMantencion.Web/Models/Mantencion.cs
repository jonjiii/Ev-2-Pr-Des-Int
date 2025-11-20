namespace SistemaMantencion.Web.Models;

public class Mantencion
{
    public int Id { get; set; }
    public int CamionetaId { get; set; }
    public Camioneta Camioneta { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = "";      // Ej: "Correctiva", "Preventiva"
    public string Detalle { get; set; } = "";
    public int Kilometraje { get; set; }
}
