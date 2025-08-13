using Microsoft.AspNetCore.Mvc;

namespace Connect4Game.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Tablero()
        {
            ViewBag.JugadorActual = "Jugador 1"; // Simulado
            return View();
        }
    }
}
