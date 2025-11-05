using System;

namespace BibliotecaWeb.Models
{
    public class Prestamo
    {
        public int PrestamoID { get; set; }
        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }
        public int LibroID { get; set; }
        public Libro Libro { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } // "Activo", "Devuelto", "Retrasado"
        public DateTime FechaVencimiento { get; set; }
        public bool TieneSancion { get; set; } = false;
        public string MotivoSancion { get; set; }
        public DateTime? FechaFinSancion { get; set; }
    }
}