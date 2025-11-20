namespace SistemaComercial.Web.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Rut { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";

    public List<Arriendo> Arriendos { get; set; } = new();
}
