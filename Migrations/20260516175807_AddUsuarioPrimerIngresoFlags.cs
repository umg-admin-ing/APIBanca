using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIBanca.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioPrimerIngresoFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_cambio_password",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "password_temporal",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requiere_cambio_password",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_cambio_password",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "password_temporal",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "requiere_cambio_password",
                table: "usuarios");
        }
    }
}
