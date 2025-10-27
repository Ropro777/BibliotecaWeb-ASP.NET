using BibliotecaWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaWeb.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new BibliotecaContext(
                serviceProvider.GetRequiredService<DbContextOptions<BibliotecaContext>>()))
            {
                // Agregar usuario de prueba si no existe
                if (!context.Usuarios.Any())
                {
                    context.Usuarios.Add(new Usuario
                    {
                        UsuarioID = 1,
                        Nombre = "Usuario Prueba",
                        Tipo = "Alumno",
                        Contraseña = "123"
                    });
                    context.SaveChanges();
                }

                // Agregar libros de prueba si no existen
                if (!context.Libros.Any())
                {
                    context.Libros.AddRange(
                        new Libro
                        {
                            Titulo = "Cien años de soledad",
                            Autor = "Gabriel García Márquez",
                            ISBN = "978-8437604947",
                            Estado = "Disponible"
                        },
                        new Libro
                        {
                            Titulo = "1984",
                            Autor = "George Orwell",
                            ISBN = "978-0451524935",
                            Estado = "Disponible"
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}