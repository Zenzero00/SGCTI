using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AnalisisPredictivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContadorColor",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContadorMonocromo",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContadorTotalPaginas",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasEstimadosAgotamientoTonner",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasEstimadosMantenimiento",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEstimadaAgotamientoTonner",
                table: "Impresoras",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEstimadaMantenimiento",
                table: "Impresoras",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NivelTonnerAmarillo",
                table: "Impresoras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NivelTonnerCian",
                table: "Impresoras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NivelTonnerMagenta",
                table: "Impresoras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NivelTonnerNegro",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaginasDesdeUltimoMantenimiento",
                table: "Impresoras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PromedioPaginasDiario",
                table: "Impresoras",
                type: "double precision",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PromedioTonnerDiario",
                table: "Impresoras",
                type: "double precision",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoMantenimiento",
                table: "Impresoras",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_DiasEstimadosAgotamientoTonner",
                table: "Impresoras",
                column: "DiasEstimadosAgotamientoTonner");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_DiasEstimadosMantenimiento",
                table: "Impresoras",
                column: "DiasEstimadosMantenimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_NivelTonnerNegro",
                table: "Impresoras",
                column: "NivelTonnerNegro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Impresoras_DiasEstimadosAgotamientoTonner",
                table: "Impresoras");

            migrationBuilder.DropIndex(
                name: "IX_Impresoras_DiasEstimadosMantenimiento",
                table: "Impresoras");

            migrationBuilder.DropIndex(
                name: "IX_Impresoras_NivelTonnerNegro",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "ContadorColor",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "ContadorMonocromo",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "ContadorTotalPaginas",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "DiasEstimadosAgotamientoTonner",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "DiasEstimadosMantenimiento",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "FechaEstimadaAgotamientoTonner",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "FechaEstimadaMantenimiento",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "NivelTonnerAmarillo",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "NivelTonnerCian",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "NivelTonnerMagenta",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "NivelTonnerNegro",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "PaginasDesdeUltimoMantenimiento",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "PromedioPaginasDiario",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "PromedioTonnerDiario",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "UltimoMantenimiento",
                table: "Impresoras");
        }
    }
}
