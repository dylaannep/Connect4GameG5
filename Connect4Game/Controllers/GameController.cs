using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Connect4Game.Controllers
{
    public class GameController : Controller
    {

//Injectar el contexto de la base de datos
        private readonly Connect4Context _context;
        public GameController(Connect4Context context)
        {
            _context = context;
        }

        //Prueba de acción para el tablero
        public IActionResult Tablero()
        {
            ViewBag.JugadorActual = "Jugador 1"; // Simulado
            return View();
        }

// Acción para ver el registro de partidas
        [HttpGet]
        public IActionResult RegistroPartida()
        {
            // Obtener todas las partidas con sus jugadores y ganador
            // Incluir los jugadores y el ganador para mostrar sus nombres
            // Ordenar por fecha descendente para mostrar las más recientes primero
            var partidas = _context.Partidas
            .Include(p => p.Jugador1)
            .Include(p => p.Jugador2)
            .Include(p => p.Ganador)
            .OrderByDescending(p => p.Fecha)
            .ToList();

            //System.Console.WriteLine("Partidas: " + partidas.Count);
// Devolver la vista con el modelo de partidas
            return View(partidas);
        }


// Acción para reiniciar una partida
        public async Task<IActionResult> ReiniciarPartida(int id)
        {
            // Buscar la partida por ID
            var partida = await _context.Partidas.FindAsync(id);
            if (partida == null)
            {
                return NotFound();
            }
            //System.Console.WriteLine("jugador 1: " + partida.Jugador1Id);
            // Crear un nuevo tablero vacío
            // Reiniciar el tablero y los turnos
            // Reiniciar el estado de la partida a EnCurso
            // Reiniciar el ganador a null
            // Guardar la nueva partida en la base de datos
            var tableroVacio = System.Text.Json.JsonSerializer.Serialize(CrearTableroVacio());
            System.Console.WriteLine("Tablero reiniciado: " + tableroVacio);
            var partidaReiniciar = new PartidaModel
            {
                Jugador1Id = partida.Jugador1Id, // Mantener los mismos jugadores
                Jugador2Id = partida.Jugador2Id,
                Tablero = tableroVacio,// Tablero vacío
                TurnoGuardado = 1, // Comienza el jugador 1
                Fecha = DateTime.Now,
                Estado = EstadoPartida.EnCurso,
                GanadorId = null, // Reiniciar ganador si es que lo había
            };

            //System.Console.WriteLine("Reiniciando partida: " + partida.Id);
            // Agregar la nueva partida reiniciada a la base de datos
            // No se elimina la partida original, se crea una nueva
            _context.Partidas.Add(partidaReiniciar);
            await _context.SaveChangesAsync();

            //System.Console.WriteLine("Partida reiniciada: " + partida.Id);

            // Redirigir a VerPartida para mostrar el tablero reiniciado
            return RedirectToAction(nameof(VerPartida), new { id = partidaReiniciar.Id });
        }


// Acción para ver una partida específica
        [HttpGet]
        public IActionResult VerPartida(int id)
        {
            // Buscar la partida por ID, incluyendo los jugadores y el ganador
            var partida = _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .FirstOrDefault(p => p.Id == id);

// Si no se encuentra la partida, retornar NotFound
            if (partida == null)
            {
                return NotFound();
            }

            // se deserializa
            // Si el tablero está vacío, crear un tablero vacío
            // Si el tablero está guardado como JSON, deserializarlo a List<List<int>>
            // Si el tablero es null o vacío, crear un tablero vacío
            var tablero = string.IsNullOrEmpty(partida.Tablero)
           ? CrearTableroVacio()
           : System.Text.Json.JsonSerializer.Deserialize<List<List<int>>>(partida.Tablero);

            //Crear el ViewModel con la partida, el tablero y el turno guardado
            // El turno guardado es el turno actual de la partida
            var vm = new VerPartidaViewModel
            {
                Partida = partida,
                Tablero = tablero,
                TurnoGuardado = partida.TurnoGuardado
            };
            // Retornar la vista con el ViewModel
            return View(vm);
        }

// Acción para reanudar una partida
        [HttpGet]
        public IActionResult Reanudar(int id)
        {
            // Buscar la partida por ID, incluyendo los jugadores y el ganador
            var partida = _context.Partidas
                .AsNoTracking() // No necesitamos rastrear cambios en esta consulta
                .FirstOrDefault(p => p.Id == id);

            // Si no se encuentra la partida, retornar NotFound
            if (partida == null) return NotFound();

            // Si ya terminó, solo muestra el detalle 
            if (partida.Estado == EstadoPartida.Finalizada)
                return RedirectToAction(nameof(VerPartida), new { id });

            // Si está en curso se ve VerPartida que ya carga tablero
            return RedirectToAction(nameof(VerPartida), new { id });
        }

        // Método auxiliar para crear un tablero vacío de 6 filas y 7 columnas
        // Representado como una lista de listas de enteros (jagged array)
        private List<List<int>> CrearTableroVacio()
        {
            // Crear un tablero vacío de 6 filas y 7 columnas
            return Enumerable.Range(0, 6).Select(_ => Enumerable.Repeat(0, 7).ToList()).ToList();
        }


        // Acción para crear una nueva partida
        [HttpGet]
        public IActionResult CrearPartida()
        {
            // Cargar la lista de jugadores desde la base de datos
            ViewBag.Jugadores = _context.Jugadores.ToList(); 
            return View();
        }

        // Acción para crear una nueva partida con dos jugadores
        // Recibe los IDs de los jugadores 1 y 2
        // Crea una nueva partida con un tablero vacío y el turno del jugador 1
        [HttpPost]
        public async Task<IActionResult> CrearPartida(int jugador1Id, int jugador2Id)
        {
            //Buscar los jugadores por sus IDs
            var jugador1 = await _context.Jugadores.FindAsync(jugador1Id);
            var jugador2 = await _context.Jugadores.FindAsync(jugador2Id);
            // Validar que ambos jugadores existen y no son el mismo
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

            // Crear un tablero vacío de 6 filas y 7 columnas
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

            // Agregar la nueva partida a la base de datos
            _context.Partidas.Add(partida);
            await _context.SaveChangesAsync();
            // ANTES:
            // return View("Tablero", partida);
            // AHORA:
            // Redirigir a VerPartida para mostrar el tablero de la nueva partida
            return RedirectToAction(nameof(VerPartida), new { id = partida.Id });
        }

        // Acción para actualizar el tablero y el turno guardado
        public async Task<IActionResult> ActualizarTablero([FromBody] ActualizarTableroDTO model)
        {
            // Validar el modelo
            var partida = await _context.Partidas.FindAsync(model.PartidaId);
            // Si no se encuentra la partida, retornar NotFound
            if (partida == null)
            {
                return NotFound("Partida no encontrada.");
            }

            // Seriaializar el tablero desde el modelo
            partida.Tablero = System.Text.Json.JsonSerializer.Serialize(model.Tablero);
            //System.Console.WriteLine(partida.Tablero);
            // Si el turno actual es 1, el siguiente será 2; si es 2, el siguiente será 1
            partida.TurnoGuardado = model.Turno == 1 ? 2 : 1;

            
            var matriz = ToRectangular(model.Tablero); // int[,] de 6x7

            //System.Console.WriteLine("Turno guardado: " + partida.TurnoGuardado);


            // Verificar ganador
            int ganadorJugador = VerificarGanador(matriz); // 0, 1 o 2
            var jugador1 = await _context.Jugadores.FindAsync(partida.Jugador1Id);
            var jugador2 = await _context.Jugadores.FindAsync(partida.Jugador2Id);

            // Ver si hay un ganador
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

// Retornar el estado de la partida y el turno guardado
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
