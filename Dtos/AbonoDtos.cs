using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateAbonoDto
{
    [Required]
    public int IdCredito { get; set; }

    [Required]
    public decimal Monto { get; set; }

    public decimal SaldoAnterior { get; set; }

    public decimal? SaldoNuevo { get; set; }
}

public class UpdateAbonoDto : CreateAbonoDto;