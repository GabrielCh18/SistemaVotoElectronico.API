using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class CambioAJuntaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos");

            migrationBuilder.DropTable(
                name: "Seccion");

            migrationBuilder.DropIndex(
                name: "IX_Votos_SeccionId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "SeccionId",
                table: "Votos");

            migrationBuilder.AddColumn<int>(
                name: "JuntaId",
                table: "Votos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Votos_JuntaId",
                table: "Votos",
                column: "JuntaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Juntas_JuntaId",
                table: "Votos",
                column: "JuntaId",
                principalTable: "Juntas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Juntas_JuntaId",
                table: "Votos");

            migrationBuilder.DropIndex(
                name: "IX_Votos_JuntaId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "JuntaId",
                table: "Votos");

            migrationBuilder.AddColumn<int>(
                name: "SeccionId",
                table: "Votos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Seccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParroquiaId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seccion_Parroquias_ParroquiaId",
                        column: x => x.ParroquiaId,
                        principalTable: "Parroquias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_SeccionId",
                table: "Votos",
                column: "SeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Seccion_ParroquiaId",
                table: "Seccion",
                column: "ParroquiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos",
                column: "SeccionId",
                principalTable: "Seccion",
                principalColumn: "Id");
        }
    }
}
