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

// CONFIGURACIÓN ÚNICA PARA SQLite (desarrollo y producción)
builder.Services.AddDbContext<BibliotecaContext>(options =>
{
    // En Render, usar path persistente; localmente usar path normal
    var dbPath = Environment.GetEnvironmentVariable("RENDER") != null 
        ? "/opt/render/project/src/biblioteca.db"
        : "biblioteca.db";
    
    options.UseSqlite($"Data Source={dbPath}");
    
    Console.WriteLine($"=== Usando SQLite en: {dbPath} ===");
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

// INICIALIZAR BASE DE DATOS AL INICIAR
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BibliotecaContext>();
        context.Database.EnsureCreated();
        
        // Agregar datos iniciales si la base está vacía
        if (!context.Usuarios.Any())
        {
            context.Usuarios.AddRange(
                new Usuario { UsuarioID = 6711630, Nombre = "Renato Alexander Cristaldo Goiris", Tipo = "Alumno", Contraseña = "alumno6711630" },
                new Usuario { UsuarioID = 6305685, Nombre = "Oscar Sebastian Martinez Benitez", Tipo = "Alumno", Contraseña = "alumno6305685" },
                new Usuario { UsuarioID = 7645183, Nombre = "Araceli Jazmin Oviedo Encina", Tipo = "Alumno", Contraseña = "alumno7645183" },
                new Usuario { UsuarioID = 7699668, Nombre = "Camila Lujan Andino Aguilar", Tipo = "Alumno", Contraseña = "alumno7699668" },
                new Usuario { UsuarioID = 6621742, Nombre = "Alex Hernan Sosa Ugarte", Tipo = "Alumno", Contraseña = "alumno6621742" },
                new Usuario { UsuarioID = 8239786, Nombre = "Rodrigo Servin Rojas", Tipo = "Alumno", Contraseña = "alumno8239786" },
                new Usuario { UsuarioID = 7425551, Nombre = "Fatima Elizabeth Vazquez Armoa", Tipo = "Alumno", Contraseña = "alumno7425551" },
                new Usuario { UsuarioID = 6609006, Nombre = "Mercedes Violeta Oviedo Odecino", Tipo = "Alumno", Contraseña = "alumno6609006" },
                new Usuario { UsuarioID = 6177459, Nombre = "Arnaldo Rubén Alfonzo Bruno", Tipo = "Alumno", Contraseña = "alumno6177459" },
                new Usuario { UsuarioID = 6535539, Nombre = "Guadalupe Lujan Portillo Portillo", Tipo = "Alumno", Contraseña = "alumno6535539" },
                new Usuario { UsuarioID = 2, Nombre = "Ana Torres", Tipo = "Profesor", Contraseña = "profe456" } // PROFESOR AGREGADO CORRECTAMENTE
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
            Console.WriteLine($"=== {context.Usuarios.Count()} USUARIOS CREADOS ===");
            Console.WriteLine($"=== {context.Libros.Count()} LIBROS CREADOS ===");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== ERROR INICIALIZANDO BD: {ex.Message} ===");
    }
}

app.Run();