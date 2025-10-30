using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using BibliotecaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaWeb.Controllers
{
    public class ReservasController : Controller
    {
        private readonly BibliotecaContext _context;

        public ReservasController(BibliotecaContext context)
        {
            _context = context;
        }

        // LISTAR RESERVAS DEL USUARIO
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            var reservas = _context.Reservas
                .Include(r => r.Libro)
                .Where(r => r.UsuarioID == usuarioID && r.Estado == "Activa")
                .OrderByDescending(r => r.FechaReserva)
                .ToList();

            return View(reservas);
        }

        // FORMULARIO PARA CREAR RESERVA
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

            var librosReservables = _context.Libros
                .Where(l => l.Estado == "Prestado" || l.Estado == "Disponible")
                .ToList();

            ViewBag.Libros = librosReservables;
            return View();
        }

        // PROCESAR RESERVA
        [HttpPost]
        public IActionResult Create(int libroID)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID").Value;
            var libro = _context.Libros.FirstOrDefault(l => l.LibroID == libroID);

            if (libro == null)
            {
                TempData["Error"] = "Libro no encontrado";
                return RedirectToAction("Create");
            }

            // Verificar si ya existe una reserva activa para este libro
            var reservaExistente = _context.Reservas
                .FirstOrDefault(r => r.LibroID == libroID && r.Estado == "Activa");

            if (reservaExistente != null)
            {
                TempData["Error"] = "Este libro ya tiene una reserva activa";
                return RedirectToAction("Create");
            }

            // Crear la reserva
            var reserva = new Reserva
            {
                UsuarioID = usuarioID,
                LibroID = libroID,
                FechaReserva = DateTime.Now,
                Estado = "Activa"
            };

            // Si el libro está disponible, marcarlo como reservado
            if (libro.Estado == "Disponible")
            {
                libro.Estado = "Reservado";
            }

            _context.Reservas.Add(reserva);
            _context.SaveChanges();

            TempData["Success"] = $"Reserva realizada: {libro.Titulo}";
            return RedirectToAction("Index");
        }

        // CANCELAR RESERVA
        public IActionResult Cancelar(int id)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var reserva = _context.Reservas
                .Include(r => r.Libro)
                .FirstOrDefault(r => r.ReservaID == id && r.Estado == "Activa");

            if (reserva == null)
            {
                TempData["Error"] = "Reserva no encontrada";
                return RedirectToAction("Index");
            }

            // Marcar como cancelada
            reserva.Estado = "Cancelada";

            // Si el libro estaba reservado, volver a disponible
            if (reserva.Libro.Estado == "Reservado")
            {
                reserva.Libro.Estado = "Disponible";
            }

            _context.SaveChanges();

            TempData["Success"] = $"Reserva cancelada: {reserva.Libro.Titulo}";
            return RedirectToAction("Index");
        }
    }
}