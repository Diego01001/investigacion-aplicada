using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservas.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El solicitante es obligatorio")]
        public string Solicitante { get; set; }
        
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Correo { get; set; }
        
        [Required]
        public int EspacioId { get; set; }
        
        [ForeignKey("EspacioId")]
        public virtual Espacio? Espacio { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; }
        
        [Required(ErrorMessage = "El motivo es obligatorio")]
        public string Motivo { get; set; }
        
        public string Estado { get; set; } = "Vigente"; // Vigente, Cancelada
    }
}
