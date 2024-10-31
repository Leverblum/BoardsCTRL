using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardsCTRL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    categoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoryStatus = table.Column<bool>(type: "bit", nullable: false),
                    createdCategoryById = table.Column<int>(type: "int", nullable: true),
                    modifiedCategoryById = table.Column<int>(type: "int", nullable: true),
                    createdCategoryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedCategoryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.categoryId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    roleStatus = table.Column<bool>(type: "bit", nullable: false),
                    createdRoleById = table.Column<int>(type: "int", nullable: true),
                    modifiedRoleById = table.Column<int>(type: "int", nullable: true),
                    createdRoleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedRoleDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    boardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    boardTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    boardDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    boardStatus = table.Column<bool>(type: "bit", nullable: false),
                    createdBoardById = table.Column<int>(type: "int", nullable: true),
                    modifiedBoardById = table.Column<int>(type: "int", nullable: true),
                    createdBoardDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedBoardDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.boardId);
                    table.ForeignKey(
                        name: "FK_Boards_Categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "categoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleId = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userStatus = table.Column<bool>(type: "bit", nullable: false),
                    createdUserBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    modifiedUserById = table.Column<int>(type: "int", nullable: true),
                    createdUserDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedUserDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slides",
                columns: table => new
                {
                    slideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    boardId = table.Column<int>(type: "int", nullable: false),
                    slideTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    time = table.Column<int>(type: "int", nullable: false),
                    slideStatus = table.Column<bool>(type: "bit", nullable: false),
                    createdSlideById = table.Column<int>(type: "int", nullable: true),
                    modifiedSlideById = table.Column<int>(type: "int", nullable: true),
                    createdSlideDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedSlideDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.slideId);
                    table.ForeignKey(
                        name: "FK_Slides_Boards_boardId",
                        column: x => x.boardId,
                        principalTable: "Boards",
                        principalColumn: "boardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_categoryId",
                table: "Boards",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Slides_boardId",
                table: "Slides",
                column: "boardId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_roleId",
                table: "Users",
                column: "roleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
