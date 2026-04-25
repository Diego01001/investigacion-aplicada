using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionReservas.Models
{
    public class Espacio
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }
        
        [Range(1, 1000, ErrorMessage = "La capacidad debe ser entre 1 y 1000")]
        public int Capacidad { get; set; }
        
        [Required(ErrorMessage = "La ubicación es obligatoria")]
        public string Ubicacion { get; set; }
        
        public string Estado { get; set; } = "Disponible"; // Disponible, Mantenimiento, Ocupado
    }
}
