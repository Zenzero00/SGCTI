using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MonitorRedSnmp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComunidadSnmp",
                table: "Impresoras",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LatenciaMs",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComunidadSnmp",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "LatenciaMs",
                table: "Impresoras");
        }
    }
}
