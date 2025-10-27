using System;

namespace BibliotecaWeb.Models
{
    public class Reserva
    {
        public int ReservaID { get; set; }
        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }
        public int LibroID { get; set; }
        public Libro Libro { get; set; }
        public DateTime FechaReserva { get; set; }
        public string Estado { get; set; } // "Activa", "Cancelada", "Atendida"
    }
}