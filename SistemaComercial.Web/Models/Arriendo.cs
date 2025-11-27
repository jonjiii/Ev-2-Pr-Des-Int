namespace SistemaComercial.Web.Models;

public class Arriendo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    // Este dato vendrá vía gRPC desde el sistema de mantención
    public string Patente { get; set; } = "";

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaTermino { get; set; }
    public decimal PrecioTotal { get; set; }
    public Factura? Factura { get; set; }
}
