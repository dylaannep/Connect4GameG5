using Microsoft.AspNetCore.Mvc;

namespace Connect4Game.Controllers
{
    public class GameController : Controller
    {

        private readonly Connect4Context _context;
        public GameController(Connect4Context context)
        {
            _context = context;
        }
        public IActionResult Tablero()
        {
            ViewBag.JugadorActual = "Jugador 1"; // Simulado
            return View();
        }

        [HttpGet]
        public IActionResult CrearPartida()
        {

            ViewBag.Jugadores = _context.Jugadores.ToList(); // Simular la obtención de jugadores
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearPartida(int jugador1Id, int jugador2Id)
        {
            var jugador1 = await _context.Jugadores.FindAsync(jugador1Id); // Simular la obtención del jugador 1
            var jugador2 = await _context.Jugadores.FindAsync(jugador2Id); // Simular la obtención del jugador 2
            if (jugador1 == null || jugador2 == null)
            {
                ModelState.AddModelError("", "Uno o ambos jugadores no existen.");
                ViewBag.Jugadores = _context.Jugadores.ToList();
                return View();
            }
            if (jugador1Id == jugador2Id)
            {
                ModelState.AddModelError("", "Los jugadores no pueden ser el mismo.");
                ViewBag.ErrorMismoJugador = "Los jugadores no pueden ser el mismo.";
                ViewBag.Jugadores = _context.Jugadores.ToList();
                return View();
            }


            var tablero = Enumerable.Range(0, 6).Select(_ => Enumerable.Repeat(0, 7).ToList()).ToList();
            var partida = new PartidaModel
            {
                Jugador1 = jugador1,
                Jugador2 = jugador2,
                Tablero = System.Text.Json.JsonSerializer.Serialize(tablero), // Tablero vacío
                TurnoGuardado = 1, // Comienza el jugador 1
                Fecha = DateTime.Now,
                Estado = EstadoPartida.EnCurso
            };

            _context.Partidas.Add(partida);
            await _context.SaveChangesAsync();

            return View("Tablero", partida);
        }

        public async Task<IActionResult> ActualizarTablero([FromBody] ActualizarTableroDTO model)
        {
            var partida = await _context.Partidas.FindAsync(model.PartidaId);
            if (partida == null)
            {
                return NotFound("Partida no encontrada.");
            }

            partida.Tablero = System.Text.Json.JsonSerializer.Serialize(model.Tablero);
            System.Console.WriteLine(partida.Tablero);
            partida.TurnoGuardado = model.Turno;
            
            _context.Partidas.Update(partida);
            await _context.SaveChangesAsync();
            System.Console.WriteLine(partida.TurnoGuardado);
            //System.Console.WriteLine(partida.Tablero);



            return Ok(new { success = true });
        }
    }
}
