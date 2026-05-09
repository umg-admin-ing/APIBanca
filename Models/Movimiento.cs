using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("movimientos")]
public class Movimiento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_movimiento")]
    public int IdMovimiento { get; set; }

    [Column("id_cuenta")]
    public int IdCuenta { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Precision(18, 2)]
    [Column("monto")]
    public decimal Monto { get; set; }

    [Column("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [Column("referencia")]
    public string Referencia { get; set; } = string.Empty;

    [Precision(18, 2)]
    [Column("saldo_resultante")]
    public decimal SaldoResultante { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdCuenta))]
    public Cuenta Cuenta { get; set; } = null!;
}