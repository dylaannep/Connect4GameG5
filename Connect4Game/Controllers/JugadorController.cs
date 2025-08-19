using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Connect4Game.Controllers
{
    public class JugadorController : Controller
    {
        private readonly Connect4Context _context;

        public JugadorController(Connect4Context context)
        {
            _context = context;
        }
        /*
                public IActionResult Index()
                {
                    ViewData["Title"] = "Lista de Jugadores";
                    return View();
                }
        */
        // Acción para mostrar la lista de jugadores
        public async Task<IActionResult> Index()
        {
            //ViewData["Title"] = "Lista de Jugadores";
// Traer la lista de jugadores desde la base de datos y ordenarla por marcador
            // También inicializar el modelo para un nuevo jugador
            var jugadores_vm = new JugadorViewModel
            {
                Jugadores = await _context.Jugadores.OrderByDescending(j => j.Marcador).ToListAsync(), // Lista de jugadores ordenada por marcador
                NuevoJugador = new JugadorModel()
            };
            return View(jugadores_vm);

        }




// Acción para crear un nuevo jugador
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Nombre,Identificacion")] JugadorModel jugador)
        public async Task<IActionResult> Create(JugadorModel jugador)
        {
            // Verificar si el modelo es válido
            if (ModelState.IsValid)
            {
                //Verificar si la identificación ya existe
                if (_context.Jugadores.Any(j => j.Identificacion == jugador.Identificacion))
                {
                    // Si la identificación ya existe, agregar un error al modelo
                    ModelState.AddModelError("Identificacion", "La identificación ya existe.");
                    // Dejar el modal abierto
                    //ViewBag.MostrarModal = true;
                    // Retornar la vista con el modelo actualizado
                    //return View("Index", jugador);
                }
                else
                {
                    _context.Jugadores.Add(jugador);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

            }
            // Si el modelo no es válido, retornar la vista con el modelo actualizado
            var jugadores_vm = new JugadorViewModel
            {
                Jugadores = await _context.Jugadores.ToListAsync(),
                NuevoJugador = jugador
            };
            // Dejar el modal abierto
            ViewBag.MostrarModal = true;
            // Retornar la vista con el modelo actualizado
            return View("Index", jugadores_vm);
        }

        public IActionResult CreateModal()
        {
            return PartialView("_CreateModal");
        }
    }
}
