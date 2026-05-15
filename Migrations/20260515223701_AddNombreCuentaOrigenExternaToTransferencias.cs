using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIBanca.Migrations
{
    /// <inheritdoc />
    public partial class AddNombreCuentaOrigenExternaToTransferencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nombre_cuenta_origen_externa",
                table: "transferencias",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nombre_cuenta_origen_externa",
                table: "transferencias");
        }
    }
}
