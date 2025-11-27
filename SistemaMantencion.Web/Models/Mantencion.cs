using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaMantencion.Web.Models;

public class Mantencion
{
    public int Id { get; set; }

    [Required]
    public int CamionetaId { get; set; }

    public Camioneta Camioneta { get; set; } = null!;

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Debe ingresar un kilometraje válido.")]
    public int Kilometraje { get; set; }

    [Required]
    public string Detalle { get; set; } = string.Empty;
}

