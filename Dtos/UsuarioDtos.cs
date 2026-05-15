using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateUsuarioDto
{
    public int? IdCliente { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
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

    public DateTime CreatedAt { get; set; }
}

public class LoginDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}