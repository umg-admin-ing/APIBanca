using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBanca.Dtos;

public class CreateTransferenciaDto
{
    public int? CuentaOrigenId { get; set; }

    public int? CuentaDestinoId { get; set; }

    public string? CuentaOrigenExterna { get; set; }

    public string? NombreCuentaOrigenExterna { get; set; }

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

public class CreateTransferenciaInternaDto
{
    [Required]
    public int CuentaOrigenId { get; set; }

    [Required]
    public string NumeroCuentaDestino { get; set; } = string.Empty;

    [Required]
    public decimal Monto { get; set; }
}

public class CreateTransferenciaInterbancariaSalienteDto
{
    [Required]
    public int CuentaOrigenId { get; set; }

    [Required]
    public string CuentaDestinoExterna { get; set; } = string.Empty;

    public string? CuentaOrigenExterna { get; set; }

    public string? SwiftOrigen { get; set; }

    [Required]
    public string SwiftDestino { get; set; } = string.Empty;

    [Required]
    public decimal Monto { get; set; }
}

public class CreateTransferenciaInterbancariaEntranteDto
{
    [Required]
    public string NumeroCuentaDestino { get; set; } = string.Empty;

    [Required]
    public string CuentaOrigenExterna { get; set; } = string.Empty;

    [Required]
    public string NombreCuentaOrigenExterna { get; set; } = string.Empty;

    [Required]
    public string SwiftOrigen { get; set; } = string.Empty;

    [Required]
    public decimal Monto { get; set; }
}

public class UpdateTransferenciaDto : CreateTransferenciaDto;

public class TransferenciaDto
{
    public int IdTransferencia { get; set; }

    public int? CuentaOrigenId { get; set; }

    public int? CuentaDestinoId { get; set; }

    public string? CuentaOrigenExterna { get; set; }

    public string? NombreCuentaOrigenExterna { get; set; }

    public string? CuentaDestinoExterna { get; set; }

    public string? SwiftOrigen { get; set; }

    public string? SwiftDestino { get; set; }

    public string? NombreClienteDestino { get; set; }

    public decimal Monto { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class ValidarTransferenciaDto
{
    [Required]
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("cuenta_destino")]
    public string CuentaDestino { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("monto")]
    public decimal Monto { get; set; }
}

public class ValidarTransferenciaRespuestaDto
{
    public string Estado { get; set; } = string.Empty;
}

