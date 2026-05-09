using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("cuentas")]
public class Cuenta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_cuenta")]
    public int IdCuenta { get; set; }

    [Column("numero_cuenta")]
    public string NumeroCuenta { get; set; } = string.Empty;

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Precision(18, 2)]
    [Column("saldo")]
    public decimal Saldo { get; set; }

    [Column("swift_banco")]
    public string SwiftBanco { get; set; } = string.Empty;

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdCliente))]
    public Cliente Cliente { get; set; } = null!;

    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    [InverseProperty(nameof(Transferencia.CuentaOrigen))]
    public ICollection<Transferencia> TransferenciasOrigen { get; set; } = new List<Transferencia>();

    [InverseProperty(nameof(Transferencia.CuentaDestino))]
    public ICollection<Transferencia> TransferenciasDestino { get; set; } = new List<Transferencia>();

    public ICollection<Credito> Creditos { get; set; } = new List<Credito>();
}