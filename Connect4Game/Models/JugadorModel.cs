
using System.ComponentModel.DataAnnotations;

public class JugadorModel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La Identificación es obligatoria.")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "La identificación debe tener exactamente 9 dígitos.")]
    //La identificación debe de ser unica
    public int Identificacion { get; set; }

    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public string Nombre { get; set; }
    public int Marcador { get; set; }
    public int Victorias { get; set; }
    public int Derrotas { get; set; }
    public int Empates { get; set; }
}