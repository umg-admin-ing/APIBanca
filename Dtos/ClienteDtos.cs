using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateClienteDto
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

    public string Estado { get; set; } = "ACTIVO";
}

public class UpdateClienteDto : CreateClienteDto;