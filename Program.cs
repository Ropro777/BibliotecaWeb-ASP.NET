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
                new Usuario { UsuarioID = 1, Nombre = "Juan Pérez", Tipo = "Alumno", Contraseña = "alumno123" },
                new Usuario { UsuarioID = 2, Nombre = "Ana Torres", Tipo = "Profesor", Contraseña = "profe456" }
            );
            
            context.Libros.AddRange(
                new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", ISBN = "978-8437604947", Estado = "Disponible" },
                new Libro { Titulo = "1984", Autor = "George Orwell", ISBN = "978-0451524935", Estado = "Disponible" },
                new Libro { Titulo = "El Quijote", Autor = "Miguel de Cervantes", ISBN = "978-8424113496", Estado = "Disponible" }
            );
            
            context.SaveChanges();
            Console.WriteLine("=== DATOS INICIALES AGREGADOS ===");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== ERROR INICIALIZANDO BD: {ex.Message} ===");
    }
}

app.Run();