namespace SistemaComercial.Web.Models;

public class Arriendo
{
    public int Id { get; set; }

    // Relación con Cliente
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    // Patente de la camioneta (desde Mantención)
    public string Patente { get; set; } = "";

    // Relación con PrecioArriendo (FK)
    public int PrecioArriendoId { get; set; }
    public PrecioArriendo PrecioArriendo { get; set; } = null!;

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaTermino { get; set; }

    // Monto total calculado en el momento de crear el arriendo
    public decimal PrecioTotal { get; set; }

    public Factura? Factura { get; set; }
}
