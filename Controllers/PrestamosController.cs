using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using BibliotecaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaWeb.Controllers
{
    public class PrestamosController : Controller
    {
        private readonly BibliotecaContext _context;

        public PrestamosController(BibliotecaContext context)
        {
            _context = context;
        }

        // LISTAR PRÉSTAMOS DEL USUARIO
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

        // VERIFICAR RETRASOS ANTES DE MOSTRAR
        VerificarRetrasos();

        var usuarioID = HttpContext.Session.GetInt32("UsuarioID");
        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => p.UsuarioID == usuarioID)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

      return View(prestamos);
}

        // FORMULARIO PARA SOLICITAR PRÉSTAMO
        public IActionResult Create(int? libroID)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Si se pasa un libroID, precargar el formulario
            if (libroID.HasValue)
            {
                var libro = _context.Libros.FirstOrDefault(l => l.LibroID == libroID.Value);
                ViewBag.LibroSeleccionado = libro;
            }

            var librosDisponibles = _context.Libros
                .Where(l => l.Estado == "Disponible")
                .ToList();

            ViewBag.Libros = librosDisponibles;
            return View();
        }

        // PROCESAR SOLICITUD DE PRÉSTAMO
        [HttpPost]
        public IActionResult Create(int libroID)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID").Value;
            var libro = _context.Libros.FirstOrDefault(l => l.LibroID == libroID);

            if (libro == null || libro.Estado != "Disponible")
            {
                TempData["Error"] = "El libro no está disponible para préstamo";
                return RedirectToAction("Create");
            }

            // Calcular fecha de vencimiento (14 días desde hoy)
            var fechaVencimiento = DateTime.Now.AddDays(14);

            // Crear el préstamo
            var prestamo = new Prestamo
            {
                UsuarioID = usuarioID,
                LibroID = libroID,
                FechaPrestamo = DateTime.Now,
                FechaVencimiento = fechaVencimiento,
                Estado = "Activo"
            };

            // Actualizar estado del libro
            libro.Estado = "Prestado";

            _context.Prestamos.Add(prestamo);
            _context.SaveChanges();

            TempData["Success"] = $"Préstamo realizado: {libro.Titulo}";
            return RedirectToAction("Index");
        }
         //MÉTODO NUEVO PARA VERIFICAR RETRASOS
           private void VerificarRetrasos()
         {
          var prestamosActivos = _context.Prestamos
          .Where(p => p.Estado == "Activo" && p.FechaVencimiento < DateTime.Now)
          .ToList();
 
          foreach (var prestamo in prestamosActivos)
         {
          prestamo.Estado = "Retrasado";
        }

          _context.SaveChanges();
         }

        // DEVOLVER LIBRO
        public IActionResult Devolver(int id)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var prestamo = _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefault(p => p.PrestamoID == id && p.Estado == "Activo");

            if (prestamo == null)
            {
                TempData["Error"] = "Préstamo no encontrado";
                return RedirectToAction("Index");
            }

            // Marcar como devuelto
            prestamo.Estado = "Devuelto";
            prestamo.FechaDevolucion = DateTime.Now;

            // Liberar el libro
            prestamo.Libro.Estado = "Disponible";

            _context.SaveChanges();

            TempData["Success"] = $"Libro devuelto: {prestamo.Libro.Titulo}";
            return RedirectToAction("Index");
        }
    }
}