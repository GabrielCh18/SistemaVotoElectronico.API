using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoProcesoElectoral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "HashIntegridad",
                table: "Votos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcesoElectoralId",
                table: "Votos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProcesoElectoralId",
                table: "Candidatos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProcesoElectorales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesoElectorales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_ProcesoElectoralId",
                table: "Votos",
                column: "ProcesoElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_ProcesoElectoralId",
                table: "Candidatos",
                column: "ProcesoElectoralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_ProcesoElectorales_ProcesoElectoralId",
                table: "Candidatos",
                column: "ProcesoElectoralId",
                principalTable: "ProcesoElectorales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_ProcesoElectorales_ProcesoElectoralId",
                table: "Votos",
                column: "ProcesoElectoralId",
                principalTable: "ProcesoElectorales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_ProcesoElectorales_ProcesoElectoralId",
                table: "Candidatos");

            migrationBuilder.DropForeignKey(
                name: "FK_Votos_Candidatos_CandidatoId",
                table: "Votos");

            migrationBuilder.DropForeignKey(
                name: "FK_Votos_ProcesoElectorales_ProcesoElectoralId",
                table: "Votos");

            migrationBuilder.DropTable(
                name: "ProcesoElectorales");

            migrationBuilder.DropIndex(
                name: "IX_Votos_ProcesoElectoralId",
                table: "Votos");

            migrationBuilder.DropIndex(
                name: "IX_Candidatos_ProcesoElectoralId",
                table: "Candidatos");

            migrationBuilder.DropColumn(
                name: "HashIntegridad",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ProcesoElectoralId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ProcesoElectoralId",
                table: "Candidatos");

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
    }
}
