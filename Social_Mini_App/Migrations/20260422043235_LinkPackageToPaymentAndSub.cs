using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class LinkPackageToPaymentAndSub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749007"),
                column: "PackageId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PackageId",
                table: "Subscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PackageId",
                table: "Payments",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_SubscriptionPackages_PackageId",
                table: "Payments",
                column: "PackageId",
                principalTable: "SubscriptionPackages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_SubscriptionPackages_PackageId",
                table: "Subscriptions",
                column: "PackageId",
                principalTable: "SubscriptionPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_SubscriptionPackages_PackageId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_SubscriptionPackages_PackageId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PackageId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PackageId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Payments");
        }
    }
}
