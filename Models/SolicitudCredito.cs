using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace APIBanca.Models;

[Table("solicitudes_credito")]
public class SolicitudCredito
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_solicitud")]
    public int IdSolicitud { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Precision(18, 2)]
    [Column("monto_solicitado")]
    public decimal MontoSolicitado { get; set; }

    [Column("plazo_meses")]
    public int PlazoMeses { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("score")]
    public int Score { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdCliente))]
    public Cliente Cliente { get; set; } = null!;

    public Credito? Credito { get; set; }
}