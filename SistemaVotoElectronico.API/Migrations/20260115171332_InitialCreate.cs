using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaVotoElectronico.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parroquias_Provincias_ProvinciaId",
                table: "Parroquias");

            migrationBuilder.DropTable(
                name: "Secciones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropColumn(
                name: "HashSeguridad",
                table: "Votos");

            migrationBuilder.RenameColumn(
                name: "SeccionId",
                table: "Votos",
                newName: "JuntaId");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Votos",
                newName: "FechaHora");

            migrationBuilder.RenameColumn(
                name: "ProvinciaId",
                table: "Parroquias",
                newName: "CantonId");

            migrationBuilder.RenameIndex(
                name: "IX_Parroquias_ProvinciaId",
                table: "Parroquias",
                newName: "IX_Parroquias_CantonId");

            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    PartidoPolitico = table.Column<string>(type: "text", nullable: false),
                    Dignidad = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cantones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    ProvinciaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cantones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cantones_Provincias_ProvinciaId",
                        column: x => x.ProvinciaId,
                        principalTable: "Provincias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotanteId = table.Column<int>(type: "integer", nullable: false),
                    CodigoUnico = table.Column<string>(type: "text", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FueUsado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    ParroquiaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zonas_Parroquias_ParroquiaId",
                        column: x => x.ParroquiaId,
                        principalTable: "Parroquias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Juntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Genero = table.Column<string>(type: "text", nullable: false),
                    ZonaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Juntas_Zonas_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cedula = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    YaVoto = table.Column<bool>(type: "boolean", nullable: false),
                    JuntaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votantes_Juntas_JuntaId",
                        column: x => x.JuntaId,
                        principalTable: "Juntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cantones_ProvinciaId",
                table: "Cantones",
                column: "ProvinciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Juntas_ZonaId",
                table: "Juntas",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CodigoUnico",
                table: "Tokens",
                column: "CodigoUnico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votantes_Cedula",
                table: "Votantes",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votantes_JuntaId",
                table: "Votantes",
                column: "JuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Zonas_ParroquiaId",
                table: "Zonas",
                column: "ParroquiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parroquias_Cantones_CantonId",
                table: "Parroquias",
                column: "CantonId",
                principalTable: "Cantones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parroquias_Cantones_CantonId",
                table: "Parroquias");

            migrationBuilder.DropTable(
                name: "Candidatos");

            migrationBuilder.DropTable(
                name: "Cantones");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Votantes");

            migrationBuilder.DropTable(
                name: "Juntas");

            migrationBuilder.DropTable(
                name: "Zonas");

            migrationBuilder.RenameColumn(
                name: "JuntaId",
                table: "Votos",
                newName: "SeccionId");

            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Votos",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "CantonId",
                table: "Parroquias",
                newName: "ProvinciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Parroquias_CantonId",
                table: "Parroquias",
                newName: "IX_Parroquias_ProvinciaId");

            migrationBuilder.AddColumn<string>(
                name: "HashSeguridad",
                table: "Votos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Secciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    ParroquiaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Secciones_Parroquias_ParroquiaId",
                        column: x => x.ParroquiaId,
                        principalTable: "Parroquias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cedula = table.Column<string>(type: "text", nullable: false),
                    CodigoAcceso = table.Column<string>(type: "text", nullable: true),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false),
                    SeccionId = table.Column<int>(type: "integer", nullable: false),
                    YaVoto = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Secciones_ParroquiaId",
                table: "Secciones",
                column: "ParroquiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parroquias_Provincias_ProvinciaId",
                table: "Parroquias",
                column: "ProvinciaId",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
