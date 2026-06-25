using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SrChauferoMVC_AzureIA.Migrations
{
    /// <inheritdoc />
    public partial class AddMesaGestionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "Mesas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraIngreso",
                table: "Mesas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Personas",
                table: "Mesas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "HoraIngreso",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "Personas",
                table: "Mesas");
        }
    }
}
