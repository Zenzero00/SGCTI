using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitacoraActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnalistaId = table.Column<int>(type: "integer", nullable: false),
                    DescripcionActividad = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EquipoIntervenido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HoraInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Etiqueta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacoraActividades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consumibles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModeloCompatible = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StockActual = table.Column<int>(type: "integer", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CostoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumibles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Impresoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    FechaInstalacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstadoPing = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComputadorasVinculadas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impresoras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetricasRed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispositivoIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    LatenciaMS = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    PaquetesPerdidos = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricasRed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioSolicitante = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Prioridad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistorialConsumo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImpresoraId = table.Column<int>(type: "integer", nullable: false),
                    ConsumibleId = table.Column<int>(type: "integer", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaginasImpresas = table.Column<int>(type: "integer", nullable: false),
                    Costo = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialConsumo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialConsumo_Consumibles_ConsumibleId",
                        column: x => x.ConsumibleId,
                        principalTable: "Consumibles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialConsumo_Impresoras_ImpresoraId",
                        column: x => x.ImpresoraId,
                        principalTable: "Impresoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraActividades_AnalistaId",
                table: "BitacoraActividades",
                column: "AnalistaId");

            migrationBuilder.CreateIndex(
                name: "IX_BitacoraActividades_Estado",
                table: "BitacoraActividades",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialConsumo_ConsumibleId",
                table: "HistorialConsumo",
                column: "ConsumibleId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialConsumo_FechaCambio",
                table: "HistorialConsumo",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialConsumo_ImpresoraId",
                table: "HistorialConsumo",
                column: "ImpresoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_Departamento",
                table: "Impresoras",
                column: "Departamento");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_Ip",
                table: "Impresoras",
                column: "Ip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetricasRed_DispositivoIP",
                table: "MetricasRed",
                column: "DispositivoIP");

            migrationBuilder.CreateIndex(
                name: "IX_MetricasRed_FechaHora",
                table: "MetricasRed",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Estado",
                table: "Tickets",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_FechaApertura",
                table: "Tickets",
                column: "FechaApertura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacoraActividades");

            migrationBuilder.DropTable(
                name: "HistorialConsumo");

            migrationBuilder.DropTable(
                name: "MetricasRed");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Consumibles");

            migrationBuilder.DropTable(
                name: "Impresoras");
        }
    }
}
