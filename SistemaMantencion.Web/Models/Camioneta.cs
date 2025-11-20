namespace SistemaMantencion.Web.Models;

public class Camioneta
{
    public int Id { get; set; }
    public string Patente { get; set; } = "";
    public int Kilometraje { get; set; }

    // Ejemplos: "Disponible", "EnArriendo", "EnMantencion"
    public string Estado { get; set; } = "Disponible";

    public bool DisponibleParaArriendo { get; set; } = true;

    public List<Mantencion> Mantenciones { get; set; } = new();
}
