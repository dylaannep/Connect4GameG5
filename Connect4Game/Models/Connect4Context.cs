using Microsoft.EntityFrameworkCore;
public class Connect4Context : DbContext
{
    public Connect4Context(DbContextOptions<Connect4Context> options) : base(options)
    {
    }

    public DbSet<JugadorModel> Jugadores { get; set; }
    public DbSet<PartidaModel> Partidas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JugadorModel>()
            .HasIndex(j => j.Identificacion)
            .IsUnique();

        modelBuilder.Entity<PartidaModel>()
        .HasOne(p => p.Jugador1)
        .WithMany()
        .HasForeignKey(p => p.Jugador1Id)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartidaModel>()
            .HasOne(p => p.Jugador2)
            .WithMany()
            .HasForeignKey(p => p.Jugador2Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartidaModel>()
            .HasOne(p => p.Ganador)
            .WithMany()
            .HasForeignKey(p => p.GanadorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}