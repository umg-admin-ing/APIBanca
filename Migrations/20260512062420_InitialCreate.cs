using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APIBanca.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    dpi = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "cuentas",
                columns: table => new
                {
                    id_cuenta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_cuenta = table.Column<string>(type: "text", nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    swift_banco = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas", x => x.id_cuenta);
                    table.ForeignKey(
                        name: "FK_cuentas_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitudes_credito",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    monto_solicitado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    plazo_meses = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitudes_credito", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK_solicitudes_credito_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    rol = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimientos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cuenta = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    referencia = table.Column<string>(type: "text", nullable: false),
                    saldo_resultante = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "FK_movimientos_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "cuentas",
                        principalColumn: "id_cuenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transferencias",
                columns: table => new
                {
                    id_transferencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cuenta_origen_id = table.Column<int>(type: "integer", nullable: true),
                    cuenta_destino_id = table.Column<int>(type: "integer", nullable: true),
                    cuenta_origen_externa = table.Column<string>(type: "text", nullable: true),
                    cuenta_destino_externa = table.Column<string>(type: "text", nullable: true),
                    swift_origen = table.Column<string>(type: "text", nullable: true),
                    swift_destino = table.Column<string>(type: "text", nullable: true),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencias", x => x.id_transferencia);
                    table.ForeignKey(
                        name: "FK_transferencias_cuentas_cuenta_destino_id",
                        column: x => x.cuenta_destino_id,
                        principalTable: "cuentas",
                        principalColumn: "id_cuenta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencias_cuentas_cuenta_origen_id",
                        column: x => x.cuenta_origen_id,
                        principalTable: "cuentas",
                        principalColumn: "id_cuenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "creditos",
                columns: table => new
                {
                    id_credito = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_solicitud = table.Column<int>(type: "integer", nullable: false),
                    id_cuenta = table.Column<int>(type: "integer", nullable: false),
                    monto_original = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tasa_interes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cuota_mensual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creditos", x => x.id_credito);
                    table.ForeignKey(
                        name: "FK_creditos_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "cuentas",
                        principalColumn: "id_cuenta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_creditos_solicitudes_credito_id_solicitud",
                        column: x => x.id_solicitud,
                        principalTable: "solicitudes_credito",
                        principalColumn: "id_solicitud",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "abonos",
                columns: table => new
                {
                    id_abono = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_credito = table.Column<int>(type: "integer", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo_nuevo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abonos", x => x.id_abono);
                    table.ForeignKey(
                        name: "FK_abonos_creditos_id_credito",
                        column: x => x.id_credito,
                        principalTable: "creditos",
                        principalColumn: "id_credito",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_abonos_id_credito",
                table: "abonos",
                column: "id_credito");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_id_cuenta",
                table: "creditos",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_creditos_id_solicitud",
                table: "creditos",
                column: "id_solicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_id_cliente",
                table: "cuentas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_numero_cuenta",
                table: "cuentas",
                column: "numero_cuenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_id_cuenta",
                table: "movimientos",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_solicitudes_credito_id_cliente",
                table: "solicitudes_credito",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_cuenta_destino_id",
                table: "transferencias",
                column: "cuenta_destino_id");

            migrationBuilder.CreateIndex(
                name: "IX_transferencias_cuenta_origen_id",
                table: "transferencias",
                column: "cuenta_origen_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_cliente",
                table: "usuarios",
                column: "id_cliente",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abonos");

            migrationBuilder.DropTable(
                name: "movimientos");

            migrationBuilder.DropTable(
                name: "transferencias");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "creditos");

            migrationBuilder.DropTable(
                name: "cuentas");

            migrationBuilder.DropTable(
                name: "solicitudes_credito");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
