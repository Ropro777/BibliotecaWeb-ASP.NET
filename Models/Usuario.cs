using System.Collections.Generic;

namespace BibliotecaWeb.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "Alumno" o "Profesor"
        public string Contraseña { get; set; }

        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}