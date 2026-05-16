using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateUsuarioDto
{
    public int? IdCliente { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Password { get; set; } = string.Empty;

    public string Rol { get; set; } = "CLIENTE";

    public string Estado { get; set; } = "ACTIVO";
}

public class UpdateUsuarioDto : CreateUsuarioDto;

public class UsuarioDto
{
    public int IdUsuario { get; set; }

    public int? IdCliente { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public bool RequiereCambioPassword { get; set; }

    public bool PasswordTemporal { get; set; }

    public DateTime? FechaCambioPassword { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class LoginDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public UsuarioDto Usuario { get; set; } = new();

    public string Role { get; set; } = string.Empty;

    public bool RequiereCambioPassword { get; set; }
}

public class CambiarPasswordTemporalDto
{
    [Required]
    [MinLength(3)]
    public string PasswordActual { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string PasswordNueva { get; set; } = string.Empty;
}