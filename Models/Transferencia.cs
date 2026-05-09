using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("transferencias")]
public class Transferencia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_transferencia")]
    public int IdTransferencia { get; set; }

    [Column("cuenta_origen_id")]
    public int? CuentaOrigenId { get; set; }

    [Column("cuenta_destino_id")]
    public int? CuentaDestinoId { get; set; }

    [Column("cuenta_origen_externa")]
    public string? CuentaOrigenExterna { get; set; }

    [Column("cuenta_destino_externa")]
    public string? CuentaDestinoExterna { get; set; }

    [Column("swift_origen")]
    public string? SwiftOrigen { get; set; }

    [Column("swift_destino")]
    public string? SwiftDestino { get; set; }

    [Precision(18, 2)]
    [Column("monto")]
    public decimal Monto { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("direccion")]
    public string Direccion { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CuentaOrigenId))]
    [InverseProperty(nameof(Cuenta.TransferenciasOrigen))]
    public Cuenta? CuentaOrigen { get; set; }

    [ForeignKey(nameof(CuentaDestinoId))]
    [InverseProperty(nameof(Cuenta.TransferenciasDestino))]
    public Cuenta? CuentaDestino { get; set; }
}