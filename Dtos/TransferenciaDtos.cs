using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateTransferenciaDto
{
    public int? CuentaOrigenId { get; set; }

    public int? CuentaDestinoId { get; set; }

    public string? CuentaOrigenExterna { get; set; }

    public string? CuentaDestinoExterna { get; set; }

    public string? SwiftOrigen { get; set; }

    public string? SwiftDestino { get; set; }

    [Required]
    public decimal Monto { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    public string Direccion { get; set; } = string.Empty;

    public string Estado { get; set; } = "PENDIENTE";
}

public class UpdateTransferenciaDto : CreateTransferenciaDto;