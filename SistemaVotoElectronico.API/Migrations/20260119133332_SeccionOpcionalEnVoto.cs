using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class SeccionOpcionalEnVoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos");

            migrationBuilder.AlterColumn<int>(
                name: "SeccionId",
                table: "Votos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos",
                column: "SeccionId",
                principalTable: "Seccion",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos");

            migrationBuilder.AlterColumn<int>(
                name: "SeccionId",
                table: "Votos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Seccion_SeccionId",
                table: "Votos",
                column: "SeccionId",
                principalTable: "Seccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
