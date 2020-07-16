using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MicrosoftGraphAPIBot.Migrations
{
    public partial class V1 : Migration
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
                    Name = table.Column<string>(nullable: false),
                    Secrets = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "AppAuths",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    RefreshToken = table.Column<string>(nullable: false),
                    Scope = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    AzureAppId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAuths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAuths_AzureApps_AzureAppId",
                        column: x => x.AzureAppId,
                        principalTable: "AzureApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAuths_AzureAppId",
                table: "AppAuths",
                column: "AzureAppId");

            migrationBuilder.CreateIndex(
                name: "IX_AzureApps_TelegramUserId",
                table: "AzureApps",
                column: "TelegramUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAuths");

            migrationBuilder.DropTable(
                name: "AzureApps");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
