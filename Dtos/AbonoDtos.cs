using System.ComponentModel.DataAnnotations;

namespace APIBanca.Dtos;

public class CreateAbonoDto
{
    [Required]
    public int IdCredito { get; set; }

    [Required]
    public decimal Monto { get; set; }

    [Required]
    [RegularExpression("(CAPITAL|CUOTA)", ErrorMessage = "TipoAbono debe ser CAPITAL o CUOTA.")]
    public string TipoAbono { get; set; } = "CUOTA";  // ← NUEVO

    public decimal SaldoAnterior { get; set; }
    public decimal? SaldoNuevo { get; set; }
}

public class UpdateAbonoDto : CreateAbonoDto;

public class AbonoDto
{
    public int IdAbono { get; set; }
    public int IdCredito { get; set; }
    public decimal Monto { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoNuevo { get; set; }
    public string TipoAbono { get; set; } = string.Empty;  // ← NUEVO
    public DateTime CreatedAt { get; set; }
}

public class AbonoResultadoDto
{
    public int IdAbono { get; set; }
    public decimal MontoAplicado { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoNuevo { get; set; }
    public string TipoAbono { get; set; } = string.Empty;  // ← NUEVO
    public decimal? NuevaCuotaMensual { get; set; }        // ← NUEVO (solo aplica en abono a capital)
    public DateTime CreatedAt { get; set; }
}
