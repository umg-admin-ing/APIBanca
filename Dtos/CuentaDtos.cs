using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateCuentaDto
{
    [Required]
    public string NumeroCuenta { get; set; } = string.Empty;

    [Required]
    public int IdCliente { get; set; }

    [Required]
    public string SwiftBanco { get; set; } = string.Empty;

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public decimal Saldo { get; set; }

    public string Estado { get; set; } = "ACTIVA";
}

public class UpdateCuentaDto : CreateCuentaDto;

public class CuentaDto
{
    public int IdCuenta { get; set; }

    public string NumeroCuenta { get; set; } = string.Empty;

    public int IdCliente { get; set; }

    public decimal Saldo { get; set; }

    public string SwiftBanco { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}