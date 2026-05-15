using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmoleraERP.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosYDireccionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentarios",
                table: "Cotizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DireccionCliente",
                table: "Cotizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NombreCliente",
                table: "Cotizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TelefonoCliente",
                table: "Cotizaciones",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comentarios",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "DireccionCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NombreCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TelefonoCliente",
                table: "Cotizaciones");
        }
    }
}
