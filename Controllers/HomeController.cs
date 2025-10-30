using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using Microsoft.AspNetCore.Http;

namespace BibliotecaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly BibliotecaContext _context;

        public HomeController(BibliotecaContext context)
        {
            _context = context;
        }

public IActionResult Index()
{
    if (HttpContext.Session.GetInt32("UsuarioID") == null)
    {
        return RedirectToAction("Index", "Login");
    }

    ViewBag.Nombre = HttpContext.Session.GetString("Nombre");
    ViewBag.Tipo = HttpContext.Session.GetString("Tipo");
    
    // ESTADÍSTICAS COMPLETAS
    ViewBag.TotalLibros = _context.Libros.Count();
    ViewBag.LibrosDisponibles = _context.Libros.Count(l => l.Estado == "Disponible");
    ViewBag.LibrosReservados = _context.Libros.Count(l => l.Estado == "Reservado");
    ViewBag.PrestamosActivos = _context.Prestamos.Count(p => p.Estado == "Activo" || p.Estado == "Retrasado");

    return View();
}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}