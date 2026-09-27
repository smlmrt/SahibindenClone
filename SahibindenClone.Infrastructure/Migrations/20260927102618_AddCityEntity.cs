using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahibindenClone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name", "CreatedAt", "IsActive" },
                values: new object[] { 1, "İstanbul", DateTime.UtcNow, true });

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Adverts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Adverts_CityId",
                table: "Adverts",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Cities_CityId",
                table: "Adverts",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Cities_CityId",
                table: "Adverts");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_CityId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Adverts");
        }
    }
}
