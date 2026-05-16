using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateCuentaDto
{
    [Required]
    public int IdCliente { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public decimal Saldo { get; set; }
}

public class UpdateCuentaDto
{
    [Required]
    public int IdCliente { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public decimal Saldo { get; set; }
}

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

public class CuentaVerificacionDto
{
    public string NumeroCuenta { get; set; } = string.Empty;

    public string NombreCliente { get; set; } = string.Empty;
}

public class AperturaCuentaClienteDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Dpi { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Telefono { get; set; } = string.Empty;
}

public class AperturaCuentaDto
{
    [Required]
    public AperturaCuentaClienteDto Cliente { get; set; } = new();

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public decimal Saldo { get; set; }
}

public class CredencialesTemporalesDto
{
    public string Username { get; set; } = string.Empty;

    public string PasswordTemporal { get; set; } = string.Empty;

    public bool RequiereCambioPassword { get; set; }
}

public class AperturaCuentaResponseDto
{
    public CuentaDto Cuenta { get; set; } = new();

    public string NombreCliente { get; set; } = string.Empty;

    public bool UsuarioCreado { get; set; }

    public CredencialesTemporalesDto? CredencialesTemporales { get; set; }
}

public class DepositoCuentaDto
{
    [Required]
    public decimal Monto { get; set; }
}

public class DepositoCuentaResponseDto
{
    public decimal Monto { get; set; }

    public string NumeroCuenta { get; set; } = string.Empty;

    public string NombreCliente { get; set; } = string.Empty;
}

public class MontoFlotanteTransferenciaDto
{
    public int IdTransferencia { get; set; }

    public string CuentaDestinoExterna { get; set; } = string.Empty;

    public string SwiftDestino { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CuentaMontoFlotanteDto
{
    public int IdCuenta { get; set; }

    public string NumeroCuenta { get; set; } = string.Empty;

    public decimal MontoFlotante { get; set; }

    public int CantidadTransferenciasPendientes { get; set; }

    public List<MontoFlotanteTransferenciaDto> Transferencias { get; set; } = [];
}