using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult RegistroPartida()
        {
            var partidas = _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .Include(p => p.Ganador)
            .OrderByDescending(p => p.Fecha)
            .ToList();

            //System.Console.WriteLine("Partidas: " + partidas.Count);

            return View(partidas);


        }

        [HttpGet]
        public IActionResult VerPartida(int id)
        {
            var partida = _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .FirstOrDefault(p => p.Id == id);

            if (partida == null)
            {
                return NotFound();
            }

            // se deserializa
            var tablero = string.IsNullOrEmpty(partida.Tablero)
           ? CrearTableroVacio()
           : System.Text.Json.JsonSerializer.Deserialize<List<List<int>>>(partida.Tablero);

            var vm = new VerPartidaViewModel
            {
                Partida = partida,
                Tablero = tablero,
                TurnoGuardado = partida.TurnoGuardado
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Reanudar(int id)
        {
            var partida = _context.Partidas
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);

            if (partida == null) return NotFound();

            // Si ya terminó, solo muestra el detalle 
            if (partida.Estado == EstadoPartida.Finalizada)
                return RedirectToAction(nameof(VerPartida), new { id });

            // Si está en curso se ve VerPartida que ya carga tablero
            return RedirectToAction(nameof(VerPartida), new { id });
        }




        private List<List<int>> CrearTableroVacio()
        {
            return Enumerable.Range(0, 6).Select(_ => Enumerable.Repeat(0, 7).ToList()).ToList();
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
            // ANTES:
            // return View("Tablero", partida);
            // AHORA:
            return RedirectToAction(nameof(VerPartida), new { id = partida.Id });
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
            // Si el turno actual es 1, el siguiente será 2; si es 2, el siguiente será 1
            partida.TurnoGuardado = model.Turno == 1 ? 2 : 1;

            var matriz = ToRectangular(model.Tablero); // int[,] de 6x7

            System.Console.WriteLine("Turno guardado: " + partida.TurnoGuardado);


            // Verificar ganador
            int ganadorJugador = VerificarGanador(matriz); // 0, 1 o 2
            var jugador1 = await _context.Jugadores.FindAsync(partida.Jugador1Id);
            var jugador2 = await _context.Jugadores.FindAsync(partida.Jugador2Id);

            if (ganadorJugador != 0)
            {
                // Mapear 1/2 -> Id real
                int ganadorId = (ganadorJugador == 1) ? partida.Jugador1Id : partida.Jugador2Id;

                partida.GanadorId = ganadorId;
                partida.Estado = EstadoPartida.Finalizada;

                if (ganadorJugador == 1)
                {
                    jugador1.Marcador += 1;
                    jugador1.Victorias += 1;

                    jugador2.Marcador -= 1;
                    jugador2.Derrotas += 1;
                }
                else
                {
                    jugador2.Marcador += 1;
                    jugador2.Victorias += 1;

                    jugador1.Marcador -= 1;
                    jugador1.Derrotas += 1;
                }
            }
            else if (EsEmpate(matriz))
            {
                // Empate: finalizar partida y sumar empates (marcador no cambia)
                partida.Estado = EstadoPartida.Finalizada;
                jugador1.Empates += 1;
                jugador2.Empates += 1;
            }

            await _context.SaveChangesAsync();
            System.Console.WriteLine("Turno guardado después de actualizar: " + partida.TurnoGuardado);

            return Ok(new
            {
                success = true,
                estado = partida.Estado.ToString(),
                ganadorId = partida.GanadorId, // null si no hay ganador todavía
                turno = partida.TurnoGuardado,
            });

        }

        private int VerificarGanador(int[,] tablero)
        {
            int filas = tablero.GetLength(0);
            int cols = tablero.GetLength(1);

            // Recorremos todas las posiciones del tablero para identificar si hay un ganador
            for (int f = 0; f < filas; f++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int jugador = tablero[f, c];
                    if (jugador == 0) continue;

                    // Fichas en Horizontal
                    if (c + 3 < cols &&
                        tablero[f, c + 1] == jugador &&
                        tablero[f, c + 2] == jugador &&
                        tablero[f, c + 3] == jugador)
                        return jugador;

                    // Fichas en Vertical
                    if (f + 3 < filas &&
                        tablero[f + 1, c] == jugador &&
                        tablero[f + 2, c] == jugador &&
                        tablero[f + 3, c] == jugador)
                        return jugador;

                    // Fichas en Diagonal ↘
                    if (f + 3 < filas && c + 3 < cols &&
                        tablero[f + 1, c + 1] == jugador &&
                        tablero[f + 2, c + 2] == jugador &&
                        tablero[f + 3, c + 3] == jugador)
                        return jugador;

                    // Fichas en Diagonal ↙
                    if (f + 3 < filas && c - 3 >= 0 &&
                        tablero[f + 1, c - 1] == jugador &&
                        tablero[f + 2, c - 2] == jugador &&
                        tablero[f + 3, c - 3] == jugador)
                        return jugador;
                }
            }

            return 0; // 0 = nadie ha ganado
        }

        private bool EsEmpate(int[,] tablero)
        {
            int filas = tablero.GetLength(0);
            int cols = tablero.GetLength(1);
            for (int f = 0; f < filas; f++)
                for (int c = 0; c < cols; c++)
                    if (tablero[f, c] == 0) return false;
            return true;
        }

        // Aca habian dos opciones por un Int como esta en el DTO o cambiar el dto y el metodo ToRectangular por int[][]
        private static int[,] ToRectangular(List<List<int>> jagged)
        {
            int rows = jagged.Count;
            int cols = jagged[0].Count;
            var rect = new int[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    rect[r, c] = jagged[r][c];
            return rect;
        }

    }
}
