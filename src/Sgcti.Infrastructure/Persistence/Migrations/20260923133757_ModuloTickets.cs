using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModuloTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UsuarioSolicitante",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "FechaApertura",
                table: "Tickets",
                newName: "FechaCreacion");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_FechaApertura",
                table: "Tickets",
                newName: "IX_Tickets_FechaCreacion");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Tickets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<int>(
                name: "SLAHoras",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Tickets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SLAHoras",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Tickets",
                newName: "FechaApertura");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_FechaCreacion",
                table: "Tickets",
                newName: "IX_Tickets_FechaApertura");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Tickets",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Tickets",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Tickets",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioSolicitante",
                table: "Tickets",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }
    }
}
