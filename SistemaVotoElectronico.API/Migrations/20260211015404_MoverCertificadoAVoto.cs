using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class MoverCertificadoAVoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificadoDescargado",
                table: "Votantes");

            migrationBuilder.AddColumn<bool>(
                name: "CertificadoDescargado",
                table: "Votos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificadoDescargado",
                table: "Votos");

            migrationBuilder.AddColumn<bool>(
                name: "CertificadoDescargado",
                table: "Votantes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
