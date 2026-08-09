using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SrChauferoMVC_AzureIA.Migrations
{
    /// <inheritdoc />
    public partial class CrearUsuariosRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });


            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    NombreUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Activo = table.Column<bool>(type: "bit", nullable: false),

                    RolId = table.Column<int>(type: "int", nullable: false)

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);

                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

        }
    }
}
