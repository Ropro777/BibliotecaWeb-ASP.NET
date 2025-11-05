using System;
using System.Collections.Generic;

namespace BibliotecaWeb.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "Alumno" o "Profesor"
        public string Contraseña { get; set; }

        // SANCIONES
        public int Sanciones { get; set; }
        public DateTime? FechaFinSancion { get; set; }
        public bool EstaSancionado => Sanciones >= 3 && (FechaFinSancion == null || FechaFinSancion > DateTime.Now);

        // RELACIONES (UNA SOLA VEZ)
        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
    }
}