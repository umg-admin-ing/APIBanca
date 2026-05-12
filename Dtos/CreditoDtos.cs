using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateCreditoDto
{
    [Required]
    public int IdSolicitud { get; set; }

    [Required]
    public int IdCuenta { get; set; }

    [Required]
    public decimal MontoOriginal { get; set; }

    [Required]
    public decimal TasaInteres { get; set; }

    [Required]
    public decimal CuotaMensual { get; set; }

    public decimal? SaldoPendiente { get; set; }

    public string Estado { get; set; } = "ACTIVO";

    public DateTime? FechaInicio { get; set; }
}

public class UpdateCreditoDto : CreateCreditoDto;

public class CreditoDto
{
    public int IdCredito { get; set; }

    public int IdSolicitud { get; set; }

    public int IdCuenta { get; set; }

    public decimal MontoOriginal { get; set; }

    public decimal SaldoPendiente { get; set; }

    public decimal TasaInteres { get; set; }

    public decimal CuotaMensual { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }
}