using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos
{
    public class AprobarSolicitudDto
    {
        [Required]
        public int IdCuenta { get; set; }          // cuenta donde se desembolsa

        [Required]
        [Range(0.01, 100)]
        public decimal TasaInteresAnual { get; set; } // ej: 18.5 => 18.5%
    }
}
