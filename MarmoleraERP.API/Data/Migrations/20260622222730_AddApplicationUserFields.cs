using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmoleraERP.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NombreCompleto",
                table: "AspNetUsers",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "AspNetUsers",
                newName: "FechaCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "AspNetUsers",
                newName: "NombreCompleto");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "AspNetUsers",
                newName: "FechaRegistro");
        }
    }
}
