using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionManualJefe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Votantes\" ADD COLUMN \"EsJefe\" boolean NOT NULL DEFAULT FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
