using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MicrosoftGraphAPIBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzureApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Secrets = table.Column<string>(nullable: false),
                    TelegramUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzureApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AzureApps_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AzureApps_TelegramUserId",
                table: "AzureApps",
                column: "TelegramUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AzureApps");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
