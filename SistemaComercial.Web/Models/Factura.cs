namespace SistemaComercial.Web.Models;

public class Factura
{
    public int Id { get; set; }
    public int ArriendoId { get; set; }
    public Arriendo Arriendo { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal Monto { get; set; }
}
