 

namespace SOS.Models
{
    
    public class AlertaEvacuacion
    {
       
        public int Id { get; set; }

   
        public string? IdUsuario { get; set; }  
 
        public DateTime FechaHoraActivacion { get; set; }
 
        public string? TipoAlerta { get; set; }

        public string? MensajeAlerta { get; set; }

       
        public decimal? LatitudActivacion { get; set; }

    
        public decimal? LongitudActivacion { get; set; }

   
        public string? DescripcionUbicacionActivacion { get; set; }
 
     
        public string? EstadoAlerta { get; set; }
 
        public int? UsuarioRegistro { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public int? UsuarioModifica { get; set; }
        public DateTime? FechaModifica { get; set; }
        public bool Activo { get; set; }
    }
}