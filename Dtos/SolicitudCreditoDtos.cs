using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateSolicitudCreditoDto
{
    [Required]
    public int IdCliente { get; set; }

    [Required]
    public decimal MontoSolicitado { get; set; }

    [Required]
    public int PlazoMeses { get; set; }

    public string Estado { get; set; } = "PENDIENTE";

    public int Score { get; set; }
}

public class UpdateSolicitudCreditoDto : CreateSolicitudCreditoDto;

public class SolicitudCreditoDto
{
    public int IdSolicitud { get; set; }

    public int IdCliente { get; set; }

    public decimal MontoSolicitado { get; set; }

    public int PlazoMeses { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }
}