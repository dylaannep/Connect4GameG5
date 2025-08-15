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
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Lista de Jugadores";

            var jugadores_vm = new JugadorViewModel
            {
                Jugadores = await _context.Jugadores.ToListAsync(),
                NuevoJugador = new JugadorModel()
            };
            return View(jugadores_vm);
            
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Nombre,Identificacion")] JugadorModel jugador)
public async Task<IActionResult> Create(JugadorModel jugador)
        {
            if (ModelState.IsValid)
            {
                //Verificar si la identificación ya existe
                if (_context.Jugadores.Any(j => j.Identificacion == jugador.Identificacion))
                {
                    ModelState.AddModelError("Identificacion", "La identificación ya existe.");
                    ViewBag.MostrarModal = true;
                    //TempData["Mensaje"] = "La identificación ya existe.";
                    return View("Index", jugador);
                }
                _context.Jugadores.Add(jugador);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                 ViewBag.MostrarModal = true;
            }
            return View("Index", jugador);
        }

        public IActionResult CreateModal()
        {
            return PartialView("_CreateModal");
        }
    }
}
