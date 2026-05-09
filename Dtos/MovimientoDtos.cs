using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateMovimientoDto
{
    [Required]
    public int IdCuenta { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    public decimal Monto { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Referencia { get; set; } = string.Empty;

    public decimal SaldoResultante { get; set; }
}

public class UpdateMovimientoDto : CreateMovimientoDto;