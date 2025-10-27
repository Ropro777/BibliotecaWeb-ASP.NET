using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using BibliotecaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaWeb.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var libros = _context.Libros.ToList();
            return View(libros);
        }

        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var libro = _context.Libros.FirstOrDefault(l => l.LibroID == id);
            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }
    }
}