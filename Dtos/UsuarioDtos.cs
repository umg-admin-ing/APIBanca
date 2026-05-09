using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateUsuarioDto
{
    [Required]
    public int IdCliente { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Rol { get; set; } = "CLIENTE";

    public string Estado { get; set; } = "ACTIVO";
}

public class UpdateUsuarioDto : CreateUsuarioDto;