using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("creditos")]
[Index(nameof(IdSolicitud), IsUnique = true)]
public class Credito
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_credito")]
    public int IdCredito { get; set; }

    [Column("id_solicitud")]
    public int IdSolicitud { get; set; }

    [Column("id_cuenta")]
    public int IdCuenta { get; set; }

    [Precision(18, 2)]
    [Column("monto_original")]
    public decimal MontoOriginal { get; set; }

    [Precision(18, 2)]
    [Column("saldo_pendiente")]
    public decimal SaldoPendiente { get; set; }

    [Precision(18, 2)]
    [Column("tasa_interes")]
    public decimal TasaInteres { get; set; }

    [Precision(18, 2)]
    [Column("cuota_mensual")]
    public decimal CuotaMensual { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("fecha_inicio")]
    public DateTime FechaInicio { get; set; }

    [ForeignKey(nameof(IdSolicitud))]
    public SolicitudCredito SolicitudCredito { get; set; } = null!;

    [ForeignKey(nameof(IdCuenta))]
    public Cuenta Cuenta { get; set; } = null!;

    public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
}