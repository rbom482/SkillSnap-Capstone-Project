using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap.Api.Migrations
{
    /// <inheritdoc />
    public partial class OptimizedPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "Projects",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiveDemoUrl",
                table: "Projects",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Technologies",
                table: "Projects",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PortfolioUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PortfolioUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name_Level",
                table: "Skills",
                columns: new[] { "Name", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Title",
                table: "Projects",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUsers_Name",
                table: "PortfolioUsers",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Skills_Name_Level",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Title",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUsers_Name",
                table: "PortfolioUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LiveDemoUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Technologies",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PortfolioUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PortfolioUsers");
        }
    }
}
