using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarmoleraERP.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaLectura",
                table: "Notificaciones");

            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "Notificaciones",
                newName: "ReferenciaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferenciaId",
                table: "Notificaciones",
                newName: "PedidoId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLectura",
                table: "Notificaciones",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
