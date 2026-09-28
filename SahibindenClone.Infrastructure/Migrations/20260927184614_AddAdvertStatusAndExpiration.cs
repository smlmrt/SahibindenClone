using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahibindenClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertStatusAndExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Adverts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Adverts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Adverts");
        }
    }
}
