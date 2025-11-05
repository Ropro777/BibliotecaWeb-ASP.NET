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

            // VERIFICAR RETRASOS Y SANCIONES
            VerificarRetrasos();
            ActualizarSanciones();

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            var prestamos = _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.UsuarioID == usuarioID)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToList();

            // Información de sanciones para la vista
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
            ViewBag.Sanciones = usuario?.Sanciones ?? 0;
            ViewBag.EstaSancionado = usuario?.EstaSancionado ?? false;

            return View(prestamos);
        }

        // FORMULARIO PARA SOLICITAR PRÉSTAMO
        public IActionResult Create(int? libroID)
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID").Value;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);

            // VERIFICAR SI EL USUARIO ESTÁ SANCIONADO
            if (usuario?.EstaSancionado == true)
            {
                TempData["Error"] = "❌ No puedes solicitar préstamos porque tienes 3 o más sanciones activas.";
                return RedirectToAction("Index", "Libros");
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
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
            var libro = _context.Libros.FirstOrDefault(l => l.LibroID == libroID);

            // VERIFICAR SANCIONES
            if (usuario?.EstaSancionado == true)
            {
                TempData["Error"] = "❌ No puedes solicitar préstamos porque tienes 3 o más sanciones activas.";
                return RedirectToAction("Index", "Libros");
            }

            if (libro == null || libro.Estado != "Disponible")
            {
                TempData["Error"] = "El libro no está disponible para préstamo";
                return RedirectToAction("Create");
            }

            // Validar límite de préstamos activos (máximo 2)
            var prestamosActivos = _context.Prestamos
                .Count(p => p.UsuarioID == usuarioID && 
                           (p.Estado == "Activo" || p.Estado == "Retrasado"));

            if (prestamosActivos >= 2)
            {
                TempData["Error"] = "Límite alcanzado: máximo 2 préstamos activos simultáneos";
                return RedirectToAction("Create");
            }

            // CALCULAR FECHA DE VENCIMIENTO (mismo día a las 15:00)
            var ahora = DateTime.Now;
            var fechaVencimiento = new DateTime(ahora.Year, ahora.Month, ahora.Day, 15, 0, 0);
            
            // Si ya pasaron las 15:00, el préstamo es para el siguiente día hábil
            if (ahora.Hour >= 15)
            {
                fechaVencimiento = fechaVencimiento.AddDays(1);
                // Si es sábado, pasar a lunes
                if (fechaVencimiento.DayOfWeek == DayOfWeek.Saturday)
                    fechaVencimiento = fechaVencimiento.AddDays(2);
                else if (fechaVencimiento.DayOfWeek == DayOfWeek.Sunday)
                    fechaVencimiento = fechaVencimiento.AddDays(1);
            }

             // Crear el préstamo
            var prestamo = new Prestamo
{
            UsuarioID = usuarioID,
            LibroID = libroID,
            FechaPrestamo = ahora,
            FechaVencimiento = fechaVencimiento,
            Estado = "Activo",
            TieneSancion = false,
            MotivoSancion = null, // Explícitamente null
            FechaFinSancion = null // Explícitamente null
};

            // Actualizar estado del libro
            libro.Estado = "Prestado";

            _context.Prestamos.Add(prestamo);
            _context.SaveChanges();

            TempData["Success"] = $"✅ Préstamo realizado: {libro.Titulo}. Devolver antes de las 15:00 del {fechaVencimiento:dd/MM/yyyy}";
            return RedirectToAction("Index");
        }

        // MÉTODO PARA VERIFICAR RETRASOS
        private void VerificarRetrasos()
        {
            var prestamosActivos = _context.Prestamos
                .Include(p => p.Usuario)
                .Where(p => p.Estado == "Activo" && p.FechaVencimiento < DateTime.Now)
                .ToList();

            foreach (var prestamo in prestamosActivos)
            {
                prestamo.Estado = "Retrasado";
                
                // Aplicar sanción automáticamente si no tiene sanción previa
                if (!prestamo.TieneSancion)
                {
                    prestamo.TieneSancion = true;
                    prestamo.MotivoSancion = "Devolución tardía";
                    
                    // Incrementar contador de sanciones del usuario
                    prestamo.Usuario.Sanciones++;
                    
                    // Si llega a 3 sanciones, aplicar sanción por 30 días
                    if (prestamo.Usuario.Sanciones >= 3)
                    {
                        prestamo.Usuario.FechaFinSancion = DateTime.Now.AddDays(30);
                    }
                }
            }

            _context.SaveChanges();
        }

        // MÉTODO PARA ACTUALIZAR SANCIONES (quitar sanciones vencidas)
        private void ActualizarSanciones()
        {
            var usuariosConSanciones = _context.Usuarios
                .Where(u => u.FechaFinSancion != null && u.FechaFinSancion <= DateTime.Now)
                .ToList();

            foreach (var usuario in usuariosConSanciones)
            {
                usuario.FechaFinSancion = null;
                // No resetear el contador de sanciones, solo quitar el bloqueo temporal
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
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.PrestamoID == id && (p.Estado == "Activo" || p.Estado == "Retrasado"));

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

            // Verificar si se aplicó sanción por retraso
            var mensaje = $"✅ Libro devuelto: {prestamo.Libro.Titulo}";
            if (prestamo.TieneSancion)
            {
                mensaje += $". ⚠️ Se aplicó 1 sanción por devolución tardía. Sanciones totales: {prestamo.Usuario.Sanciones}/3";
                
                if (prestamo.Usuario.Sanciones >= 3)
                {
                    mensaje += $". 🚫 Bloqueado hasta el {prestamo.Usuario.FechaFinSancion:dd/MM/yyyy}";
                }
            }

            _context.SaveChanges();

            TempData["Success"] = mensaje;
            return RedirectToAction("Index");
        }

        // MÉTODO PARA VER SANCIONES
        public IActionResult Sanciones()
        {
            if (HttpContext.Session.GetInt32("UsuarioID") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioID = HttpContext.Session.GetInt32("UsuarioID");
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == usuarioID);
            var prestamosSancionados = _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.UsuarioID == usuarioID && p.TieneSancion)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToList();

            ViewBag.Usuario = usuario;
            ViewBag.PrestamosSancionados = prestamosSancionados;

            return View();
        }
    }
}