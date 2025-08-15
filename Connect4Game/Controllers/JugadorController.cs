using Microsoft.AspNetCore.Mvc;

namespace Connect4Game.Controllers
{
    public class JugadorController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Lista de Jugadores";
            return View();
        }

        public IActionResult CreateModal()
{
    return PartialView("_CreateModal");
}
    }

    
}
