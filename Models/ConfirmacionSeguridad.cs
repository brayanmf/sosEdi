using System.ComponentModel.DataAnnotations;
 

namespace SOS.Models
{
     
    public class ConfirmacionSeguridad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public int? AlertaEvacuacionId { get; set; }

        public DateTime? FechaHoraConfirmacion { get; set; }

    
        public decimal? Latitud { get; set; }
 
        public decimal? Longitud { get; set; }

        [StringLength(255)]
        public string? EstadoReportado { get; set; }

        public string? Comentario { get; set; }

        // Campos de auditoría
        public int? UsuarioRegistro { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public int? UsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public bool Activo { get; set; }
    }
}