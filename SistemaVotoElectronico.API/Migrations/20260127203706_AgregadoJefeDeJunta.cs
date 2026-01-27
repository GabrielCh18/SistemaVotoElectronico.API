using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoJefeDeJunta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Votos_Juntas_JuntaId",
            //    table: "Votos");

            //migrationBuilder.DropColumn(
            //    name: "HashIntegridad",
            //    table: "Votos");

            //migrationBuilder.RenameColumn(
            //    name: "JuntaId",
            //    table: "Votos",
            //    newName: "IdVotante");

            //migrationBuilder.RenameColumn(
            //    name: "Fecha",
            //    table: "Votos",
            //    newName: "FechaVoto");

            //migrationBuilder.RenameIndex(
            //    name: "IX_Votos_JuntaId",
            //    table: "Votos",
            //    newName: "IX_Votos_IdVotante");

            //migrationBuilder.AddColumn<bool>(
            //    name: "EsJefe",
            //    table: "Votantes",
            //    type: "boolean",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Votos_Votantes_IdVotante",
            //    table: "Votos",
            //    column: "IdVotante",
            //    principalTable: "Votantes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Votantes_IdVotante",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "EsJefe",
                table: "Votantes");

            migrationBuilder.RenameColumn(
                name: "IdVotante",
                table: "Votos",
                newName: "JuntaId");

            migrationBuilder.RenameColumn(
                name: "FechaVoto",
                table: "Votos",
                newName: "Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_Votos_IdVotante",
                table: "Votos",
                newName: "IX_Votos_JuntaId");

            migrationBuilder.AddColumn<string>(
                name: "HashIntegridad",
                table: "Votos",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Juntas_JuntaId",
                table: "Votos",
                column: "JuntaId",
                principalTable: "Juntas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
