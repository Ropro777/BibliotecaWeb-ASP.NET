using Microsoft.EntityFrameworkCore;
using BibliotecaWeb.Data;
using BibliotecaWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CONFIGURACIÓN PARA RAILWAY
builder.Services.AddDbContext<BibliotecaContext>(options =>
{
    var dbPath = "biblioteca.db";
    options.UseSqlite($"Data Source={dbPath}");
    Console.WriteLine($"=== Base de datos en: {dbPath} ===");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ⚠️ RESET COMPLETO DE BASE DE DATOS ⚠️
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BibliotecaContext>();
        
        Console.WriteLine("=== INICIANDO RESET DE BASE DE DATOS ===");
        // ELIMINAR BASE DE DATOS EXISTENTE
        context.Database.EnsureDeleted();
        Console.WriteLine("=== BASE DE DATOS ELIMINADA ===");
        
        // CREAR NUEVA BASE DE DATOS
        context.Database.EnsureCreated();
        Console.WriteLine("=== NUEVA BASE DE DATOS CREADA ===");
        
        // AGREGAR DATOS INICIALES
        if (!context.Usuarios.Any())
        {
            Console.WriteLine("=== AGREGANDO DATOS INICIALES ===");
            
            context.Usuarios.AddRange(
                new Usuario { UsuarioID = 6711630, Nombre = "Renato Alexander Cristaldo Goiris", Tipo = "Alumno", Contraseña = "alumno6711630", Sanciones = 0 },
                new Usuario { UsuarioID = 6305685, Nombre = "Oscar Sebastian Martinez Benitez", Tipo = "Alumno", Contraseña = "alumno6305685", Sanciones = 0 },
                new Usuario { UsuarioID = 7645183, Nombre = "Araceli Jazmin Oviedo Encina", Tipo = "Alumno", Contraseña = "alumno7645183", Sanciones = 0 },
                new Usuario { UsuarioID = 7699668, Nombre = "Camila Lujan Andino Aguilar", Tipo = "Alumno", Contraseña = "alumno7699668", Sanciones = 0 },
                new Usuario { UsuarioID = 6621742, Nombre = "Alex Hernan Sosa Ugarte", Tipo = "Alumno", Contraseña = "alumno6621742", Sanciones = 0 },
                new Usuario { UsuarioID = 8239786, Nombre = "Rodrigo Servin Rojas", Tipo = "Alumno", Contraseña = "alumno8239786", Sanciones = 0 },
                new Usuario { UsuarioID = 7425551, Nombre = "Fatima Elizabeth Vazquez Armoa", Tipo = "Alumno", Contraseña = "alumno7425551", Sanciones = 0 },
                new Usuario { UsuarioID = 6609006, Nombre = "Mercedes Violeta Oviedo Odecino", Tipo = "Alumno", Contraseña = "alumno6609006", Sanciones = 0 },
                new Usuario { UsuarioID = 6177459, Nombre = "Arnaldo Rubén Alfonzo Bruno", Tipo = "Alumno", Contraseña = "alumno6177459", Sanciones = 0 },
                new Usuario { UsuarioID = 6535539, Nombre = "Guadalupe Lujan Portillo Portillo", Tipo = "Alumno", Contraseña = "alumno6535539", Sanciones = 0 },
                new Usuario { UsuarioID = 2, Nombre = "Ana Torres", Tipo = "Profesor", Contraseña = "profe456", Sanciones = 0 }
            );
            
            context.Libros.AddRange(
                new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", ISBN = "978-8437604947", Estado = "Disponible" },
                new Libro { Titulo = "1984", Autor = "George Orwell", ISBN = "978-0451524935", Estado = "Disponible" },
                new Libro { Titulo = "El Quijote", Autor = "Miguel de Cervantes", ISBN = "978-8424113496", Estado = "Disponible" },
                new Libro { Titulo = "Matemática Algebra", Autor = "Joaquín R.B", ISBN = "978-001", Estado = "Disponible" },
                new Libro { Titulo = "Matemática 3", Autor = "Rivera León Sanchez Carilla", ISBN = "978-002", Estado = "Disponible" },
                new Libro { Titulo = "Informática Básica", Autor = "Gallego Louoy Manzilla", ISBN = "978-003", Estado = "Disponible" },
                new Libro { Titulo = "Programación en Fortram-1ª", Autor = "Salle Bonanova", ISBN = "978-004", Estado = "Disponible" },
                new Libro { Titulo = "Física al alcance de todos 2da edición", Autor = "Juan I. Mengral", ISBN = "978-005", Estado = "Disponible" },
                new Libro { Titulo = "Aprender física 1", Autor = "Jorge Rubinstein", ISBN = "978-006", Estado = "Disponible" }
            );
            
            context.SaveChanges();
            Console.WriteLine("=== DATOS INICIALES AGREGADOS ===");
        }
        else
        {
            Console.WriteLine("=== DATOS YA EXISTÍAN, NO SE AGREGARON NUEVOS ===");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== ERROR CRÍTICO: {ex.Message} ===");
        Console.WriteLine($"=== STACK TRACE: {ex.StackTrace} ===");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");