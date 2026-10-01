using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SahibindenClone.Infrastructure.Context;

#nullable disable

namespace SahibindenClone.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261001113000_AddMarketplaceEnhancements")]
public sealed class AddMarketplaceEnhancements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Brand", table: "Adverts", type: "TEXT", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Model", table: "Adverts", type: "TEXT", maxLength: 100, nullable: true);

        migrationBuilder.CreateTable(
            name: "PurchaseTransactions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                AdvertId = table.Column<int>(type: "INTEGER", nullable: false),
                BuyerId = table.Column<int>(type: "INTEGER", nullable: false),
                SellerId = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PurchaseTransactions", x => x.Id);
                table.ForeignKey("FK_PurchaseTransactions_Adverts_AdvertId", x => x.AdvertId, "Adverts", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_PurchaseTransactions_Users_BuyerId", x => x.BuyerId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_PurchaseTransactions_Users_SellerId", x => x.SellerId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AdminAuditLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                AdminUserId = table.Column<int>(type: "INTEGER", nullable: false),
                Action = table.Column<string>(type: "TEXT", nullable: false),
                TargetType = table.Column<string>(type: "TEXT", nullable: false),
                TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                Details = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                table.ForeignKey("FK_AdminAuditLogs_Users_AdminUserId", x => x.AdminUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AdvertChangeHistories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                AdvertId = table.Column<int>(type: "INTEGER", nullable: false),
                ChangedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                PreviousTitle = table.Column<string>(type: "TEXT", nullable: false),
                NewTitle = table.Column<string>(type: "TEXT", nullable: false),
                PreviousDescription = table.Column<string>(type: "TEXT", nullable: false),
                NewDescription = table.Column<string>(type: "TEXT", nullable: false),
                PreviousPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                NewPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdvertChangeHistories", x => x.Id);
                table.ForeignKey("FK_AdvertChangeHistories_Adverts_AdvertId", x => x.AdvertId, "Adverts", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_AdvertChangeHistories_Users_ChangedByUserId", x => x.ChangedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SellerReviews",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                PurchaseTransactionId = table.Column<int>(type: "INTEGER", nullable: false),
                ReviewerId = table.Column<int>(type: "INTEGER", nullable: false),
                SellerId = table.Column<int>(type: "INTEGER", nullable: false),
                Rating = table.Column<int>(type: "INTEGER", nullable: false),
                Comment = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SellerReviews", x => x.Id);
                table.ForeignKey("FK_SellerReviews_PurchaseTransactions_PurchaseTransactionId", x => x.PurchaseTransactionId, "PurchaseTransactions", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_SellerReviews_Users_ReviewerId", x => x.ReviewerId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_SellerReviews_Users_SellerId", x => x.SellerId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_AdminAuditLogs_AdminUserId", table: "AdminAuditLogs", column: "AdminUserId");
        migrationBuilder.CreateIndex(name: "IX_AdvertChangeHistories_AdvertId", table: "AdvertChangeHistories", column: "AdvertId");
        migrationBuilder.CreateIndex(name: "IX_AdvertChangeHistories_ChangedByUserId", table: "AdvertChangeHistories", column: "ChangedByUserId");
        migrationBuilder.CreateIndex(name: "IX_PurchaseTransactions_BuyerId", table: "PurchaseTransactions", column: "BuyerId");
        migrationBuilder.CreateIndex(name: "IX_PurchaseTransactions_SellerId", table: "PurchaseTransactions", column: "SellerId");
        migrationBuilder.CreateIndex(name: "IX_SellerReviews_PurchaseTransactionId", table: "SellerReviews", column: "PurchaseTransactionId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_SellerReviews_PurchaseTransactionId_ReviewerId", table: "SellerReviews", columns: new[] { "PurchaseTransactionId", "ReviewerId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_SellerReviews_ReviewerId", table: "SellerReviews", column: "ReviewerId");
        migrationBuilder.CreateIndex(name: "IX_SellerReviews_SellerId", table: "SellerReviews", column: "SellerId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AdminAuditLogs");
        migrationBuilder.DropTable("AdvertChangeHistories");
        migrationBuilder.DropTable("SellerReviews");
        migrationBuilder.DropTable("PurchaseTransactions");
        migrationBuilder.DropColumn(name: "Brand", table: "Adverts");
        migrationBuilder.DropColumn(name: "Model", table: "Adverts");
    }
}
