using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIBanca.Models;

[Table("clientes")]
public class Cliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("dpi")]
    public string Dpi { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }

    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();

    public ICollection<SolicitudCredito> SolicitudesCredito { get; set; } = new List<SolicitudCredito>();
}