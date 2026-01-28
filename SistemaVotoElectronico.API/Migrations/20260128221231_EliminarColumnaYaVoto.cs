using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class EliminarColumnaYaVoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "YaVoto",
                table: "Votantes");

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "Votos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos");

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "Votos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "YaVoto",
                table: "Votantes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id");
        }
    }
}
