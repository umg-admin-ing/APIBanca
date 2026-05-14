using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("abonos")]
public class Abono
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_abono")]
    public int IdAbono { get; set; }

    [Column("id_credito")]
    public int IdCredito { get; set; }

    [Precision(18, 2)]
    [Column("monto")]
    public decimal Monto { get; set; }

    [Precision(18, 2)]
    [Column("saldo_anterior")]
    public decimal SaldoAnterior { get; set; }

    [Precision(18, 2)]
    [Column("saldo_nuevo")]
    public decimal SaldoNuevo { get; set; }

    [Column("tipo_abono")]          // ← NUEVO
    public string TipoAbono { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdCredito))]
    public Credito Credito { get; set; } = null!;
}