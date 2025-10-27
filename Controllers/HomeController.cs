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
    // DEBUG: Verificar estado de la sesión
    var sessionUserID = HttpContext.Session.GetInt32("UsuarioID");
    var sessionNombre = HttpContext.Session.GetString("Nombre");
    
    System.Console.WriteLine($"=== HOME CONTROLLER ===");
    System.Console.WriteLine($"Session UsuarioID: {sessionUserID}");
    System.Console.WriteLine($"Session Nombre: {sessionNombre}");
    System.Console.WriteLine($"URL actual: {HttpContext.Request.Path}");
    
    // Verificar si el usuario está logueado
    if (sessionUserID == null)
    {
        System.Console.WriteLine("=== REDIRIGIENDO A LOGIN ===");
        return RedirectToAction("Index", "Login");
    }

    ViewBag.Nombre = sessionNombre;
    ViewBag.Tipo = HttpContext.Session.GetString("Tipo");
    
    // Obtener estadísticas para el dashboard
    ViewBag.TotalLibros = _context.Libros.Count();
    ViewBag.LibrosDisponibles = _context.Libros.Count(l => l.Estado == "Disponible");
    ViewBag.PrestamosActivos = _context.Prestamos.Count(p => p.Estado == "Activo");

    System.Console.WriteLine("=== MOSTRANDO DASHBOARD ===");
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