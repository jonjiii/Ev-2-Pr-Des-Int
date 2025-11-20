namespace SistemaComercial.Web.Models;

public class PrecioArriendo
{
    public int Id { get; set; }
    public string TipoCamioneta { get; set; } = "";
    public decimal PrecioPorDia { get; set; }
}
