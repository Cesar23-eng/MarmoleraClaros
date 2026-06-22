using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmoleraERP.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncNotificacionesYUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DestinoRol",
                table: "Notificaciones",
                newName: "RolDestino");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Notificaciones",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Notificaciones");

            migrationBuilder.RenameColumn(
                name: "RolDestino",
                table: "Notificaciones",
                newName: "DestinoRol");
        }
    }
}
