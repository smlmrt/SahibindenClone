using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SahibindenClone.Infrastructure.Context;

#nullable disable

namespace SahibindenClone.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260930150000_AddUserRole")]
public sealed class AddUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<string>(
        name: "Role", table: "Users", type: "TEXT", nullable: false, defaultValue: "User");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
        name: "Role", table: "Users");
}
