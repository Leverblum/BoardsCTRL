using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardsCTRL.Migrations
{
    /// <inheritdoc />
    public partial class NuevaMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "editedUserBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "createdSlideBy",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "editedSlideBy",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "createdRoleBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "editedRoleBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "createdCategoryBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "editedCategoryBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "createdBoardBy",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "descriptionBoard",
                table: "Boards");

            migrationBuilder.RenameColumn(
                name: "statusUser",
                table: "Users",
                newName: "userStatus");

            migrationBuilder.RenameColumn(
                name: "editedUserDate",
                table: "Users",
                newName: "modifiedUserDate");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "Slides",
                newName: "URL");

            migrationBuilder.RenameColumn(
                name: "titleSlide",
                table: "Slides",
                newName: "slideTitle");

            migrationBuilder.RenameColumn(
                name: "statusSlide",
                table: "Slides",
                newName: "slideStatus");

            migrationBuilder.RenameColumn(
                name: "editedSlideDate",
                table: "Slides",
                newName: "modifiedSlideDate");

            migrationBuilder.RenameColumn(
                name: "statusRole",
                table: "Roles",
                newName: "roleStatus");

            migrationBuilder.RenameColumn(
                name: "editedRoleDate",
                table: "Roles",
                newName: "modifiedRoleDate");

            migrationBuilder.RenameColumn(
                name: "titleCategory",
                table: "Categories",
                newName: "categoryTitle");

            migrationBuilder.RenameColumn(
                name: "statusCategory",
                table: "Categories",
                newName: "categoryStatus");

            migrationBuilder.RenameColumn(
                name: "editedCaregoryDate",
                table: "Categories",
                newName: "modifiedCategoryDate");

            migrationBuilder.RenameColumn(
                name: "titleBoard",
                table: "Boards",
                newName: "boardTitle");

            migrationBuilder.RenameColumn(
                name: "statusBoard",
                table: "Boards",
                newName: "boardStatus");

            migrationBuilder.RenameColumn(
                name: "editedBoardDate",
                table: "Boards",
                newName: "modifiedBoardDate");

            migrationBuilder.RenameColumn(
                name: "editedBoardBy",
                table: "Boards",
                newName: "boardDescription");

            migrationBuilder.AddColumn<int>(
                name: "modifiedUserById",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "createdSlideById",
                table: "Slides",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "modifiedSlideById",
                table: "Slides",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "createdRoleById",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "modifiedRoleById",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "createdCategoryById",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "modifiedCategoryById",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "createdBoardById",
                table: "Boards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "modifiedBoardById",
                table: "Boards",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "modifiedUserById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "createdSlideById",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "modifiedSlideById",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "createdRoleById",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "modifiedRoleById",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "createdCategoryById",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "modifiedCategoryById",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "createdBoardById",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "modifiedBoardById",
                table: "Boards");

            migrationBuilder.RenameColumn(
                name: "userStatus",
                table: "Users",
                newName: "statusUser");

            migrationBuilder.RenameColumn(
                name: "modifiedUserDate",
                table: "Users",
                newName: "editedUserDate");

            migrationBuilder.RenameColumn(
                name: "URL",
                table: "Slides",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "slideTitle",
                table: "Slides",
                newName: "titleSlide");

            migrationBuilder.RenameColumn(
                name: "slideStatus",
                table: "Slides",
                newName: "statusSlide");

            migrationBuilder.RenameColumn(
                name: "modifiedSlideDate",
                table: "Slides",
                newName: "editedSlideDate");

            migrationBuilder.RenameColumn(
                name: "roleStatus",
                table: "Roles",
                newName: "statusRole");

            migrationBuilder.RenameColumn(
                name: "modifiedRoleDate",
                table: "Roles",
                newName: "editedRoleDate");

            migrationBuilder.RenameColumn(
                name: "modifiedCategoryDate",
                table: "Categories",
                newName: "editedCaregoryDate");

            migrationBuilder.RenameColumn(
                name: "categoryTitle",
                table: "Categories",
                newName: "titleCategory");

            migrationBuilder.RenameColumn(
                name: "categoryStatus",
                table: "Categories",
                newName: "statusCategory");

            migrationBuilder.RenameColumn(
                name: "modifiedBoardDate",
                table: "Boards",
                newName: "editedBoardDate");

            migrationBuilder.RenameColumn(
                name: "boardTitle",
                table: "Boards",
                newName: "titleBoard");

            migrationBuilder.RenameColumn(
                name: "boardStatus",
                table: "Boards",
                newName: "statusBoard");

            migrationBuilder.RenameColumn(
                name: "boardDescription",
                table: "Boards",
                newName: "editedBoardBy");

            migrationBuilder.AddColumn<string>(
                name: "editedUserBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "createdSlideBy",
                table: "Slides",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "editedSlideBy",
                table: "Slides",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "createdRoleBy",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "editedRoleBy",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "createdCategoryBy",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "editedCategoryBy",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "createdBoardBy",
                table: "Boards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descriptionBoard",
                table: "Boards",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
