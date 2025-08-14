using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public enum EstadoPartida
{
    EnCurso,
    Finalizada
}

public class PartidaModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Jugador1Id { get; set; }
    [ForeignKey("Jugador1Id")]
    public JugadorModel Jugador1 { get; set; }

    [Required]
    public int Jugador2Id { get; set; }
    [ForeignKey("Jugador2Id")]
    public JugadorModel Jugador2 { get; set; }

    [Required]
    public string Tablero { get; set; } // JSON que representa la matriz

    public int TurnoGuardado { get; set; }

    [Required]
    public EstadoPartida Estado { get; set; }

    public DateTime Fecha { get; set; }
}