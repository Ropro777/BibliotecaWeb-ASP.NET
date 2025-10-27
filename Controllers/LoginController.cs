using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using BibliotecaWeb.Models;
using Microsoft.AspNetCore.Http;

namespace BibliotecaWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly BibliotecaContext _context;

        public LoginController(BibliotecaContext context)
        {
            _context = context;
            EnsureUserExists();
        }

        private void EnsureUserExists()
        {
            // Verificar y crear usuario si no existe
            if (!_context.Usuarios.Any(u => u.UsuarioID == 1))
            {
                var usuario = new Usuario
                {
                    UsuarioID = 1,
                    Nombre = "Usuario Prueba",
                    Tipo = "Alumno",
                    Contraseña = "123"
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                System.Console.WriteLine($"=== USUARIO CREADO: {usuario.UsuarioID}:{usuario.Nombre} ===");
            }
        }

        public IActionResult Index()
        {
            // Mostrar información de debug en la página
            var userCount = _context.Usuarios.Count();
            ViewBag.DebugInfo = $"Usuarios en DB: {userCount}";
            return View();
        }

        [HttpPost]
public IActionResult Login(int usuarioID, string contraseña)
{
    var usuario = _context.Usuarios
        .FirstOrDefault(u => u.UsuarioID == usuarioID && u.Contraseña == contraseña);

    if (usuario != null)
    {
        HttpContext.Session.SetInt32("UsuarioID", usuario.UsuarioID);
        HttpContext.Session.SetString("Nombre", usuario.Nombre);
        HttpContext.Session.SetString("Tipo", usuario.Tipo);
        
        // DEBUG: Verificar que las variables de sesión se están guardando
        var sessionUserID = HttpContext.Session.GetInt32("UsuarioID");
        var sessionNombre = HttpContext.Session.GetString("Nombre");
        
        System.Console.WriteLine($"=== LOGIN EXITOSO ===");
        System.Console.WriteLine($"Session UsuarioID: {sessionUserID}");
        System.Console.WriteLine($"Session Nombre: {sessionNombre}");
        System.Console.WriteLine($"Redirigiendo a /Home/Index...");
        
        // Redirigir explícitamente
        return RedirectToAction("Index", "Home");
    }

    ViewBag.Error = "Credenciales incorrectas";
    ViewBag.DebugInfo = $"Usuarios en DB: {_context.Usuarios.Count()}";
    return View("Index");
}
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        // Método de debug
        public string Debug()
        {
            var users = _context.Usuarios.ToList();
            return $"Total usuarios: {users.Count}\n" +
                   string.Join("\n", users.Select(u => $"- {u.UsuarioID}: {u.Nombre} ({u.Tipo}) Pass: {u.Contraseña}"));
        }
public IActionResult VerUsuarios()
{
    var usuarios = _context.Usuarios.ToList();
    string resultado = "USUARIOS EN LA BASE DE DATOS:\n\n";
    
    foreach (var usuario in usuarios)
    {
        resultado += $"ID: {usuario.UsuarioID}\n";
        resultado += $"Nombre: {usuario.Nombre}\n";
        resultado += $"Tipo: {usuario.Tipo}\n";
        resultado += $"Contraseña: '{usuario.Contraseña}'\n";
        resultado += "------------------------\n";
    }
    
    return Content(resultado);
}
    }

}
