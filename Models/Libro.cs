using System.Collections.Generic;

namespace BibliotecaWeb.Models
{
    public class Libro
    {
        public int LibroID { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public string Estado { get; set; } // "Disponible", "Prestado", "Reservado"

        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}