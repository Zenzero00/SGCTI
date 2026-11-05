using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgcti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarTicketsHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "Departamento" character varying(100);
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "Categoria" character varying(100);
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "EquipoAfectado" character varying(150);
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "SolucionAplicada" character varying(2000);
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "SuministrosUtilizados" character varying(1000);
                ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "FechaResolucion" timestamp with time zone;
                CREATE INDEX IF NOT EXISTS "IX_Tickets_Departamento" ON "Tickets" ("Departamento");
                CREATE INDEX IF NOT EXISTS "IX_Tickets_Categoria" ON "Tickets" ("Categoria");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Tickets_Departamento";
                DROP INDEX IF EXISTS "IX_Tickets_Categoria";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "Departamento";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "Categoria";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "EquipoAfectado";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "SolucionAplicada";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "SuministrosUtilizados";
                ALTER TABLE "Tickets" DROP COLUMN IF EXISTS "FechaResolucion";
                """);
        }
    }
}
