using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GitHubIssueLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueUrl = table.Column<string>(type: "TEXT", nullable: false),
                    IssueNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IssueTitle = table.Column<string>(type: "TEXT", nullable: false),
                    IssueState = table.Column<string>(type: "TEXT", nullable: false),
                    IssueCreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IssueUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubIssueLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubIssueLinks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedUserId", "Description", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 1, "This is the first task", "Pending", "First Task" },
                    { 2, 2, "This is the second task", "Completed", "Second Task" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "hashedpassword", "Admin", "admin" },
                    { 2, "hashedpassword", "User", "user1" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubIssueLinks_TaskId",
                table: "GitHubIssueLinks",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitHubIssueLinks");

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
