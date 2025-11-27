using System.Collections.Generic;

namespace SistemaMantencion.Web.Models;

public class Camioneta
{
    public int Id { get; set; }
    public string Patente { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public string Estado { get; set; } = "Disponible";
    public bool DisponibleParaArriendo { get; set; } = true;

    public ICollection<Mantencion> Mantenciones { get; set; } = new List<Mantencion>();
}

