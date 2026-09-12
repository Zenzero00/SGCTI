using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlertasDinamicasConsumibles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlertaStockCritico",
                table: "Impresoras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ConsumibleTonerId",
                table: "Impresoras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadActual",
                table: "Consumibles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasAntelacionPedido",
                table: "Consumibles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_AlertaStockCritico",
                table: "Impresoras",
                column: "AlertaStockCritico");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_ConsumibleTonerId",
                table: "Impresoras",
                column: "ConsumibleTonerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Impresoras_Consumibles_ConsumibleTonerId",
                table: "Impresoras",
                column: "ConsumibleTonerId",
                principalTable: "Consumibles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Impresoras_Consumibles_ConsumibleTonerId",
                table: "Impresoras");

            migrationBuilder.DropIndex(
                name: "IX_Impresoras_AlertaStockCritico",
                table: "Impresoras");

            migrationBuilder.DropIndex(
                name: "IX_Impresoras_ConsumibleTonerId",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "AlertaStockCritico",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "ConsumibleTonerId",
                table: "Impresoras");

            migrationBuilder.DropColumn(
                name: "CantidadActual",
                table: "Consumibles");

            migrationBuilder.DropColumn(
                name: "DiasAntelacionPedido",
                table: "Consumibles");
        }
    }
}
