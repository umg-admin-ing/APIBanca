using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIBanca.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoAbonoToAbonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_abono",
                table: "abonos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo_abono",
                table: "abonos");
        }
    }
}
