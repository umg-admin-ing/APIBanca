using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos
{
    public class DenegarSolicitudDto
    {
        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;
    }
}
