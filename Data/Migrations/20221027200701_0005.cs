using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarLink_Contact.Data.Migrations
{
    public partial class _0005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatagoryContact_Categories_CategoriesId",
                table: "CatagoryContact");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserId",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Catagories");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_AppUserId",
                table: "Catagories",
                newName: "IX_Catagories_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Catagories",
                table: "Catagories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Catagories_AspNetUsers_AppUserId",
                table: "Catagories",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatagoryContact_Catagories_CategoriesId",
                table: "CatagoryContact",
                column: "CategoriesId",
                principalTable: "Catagories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catagories_AspNetUsers_AppUserId",
                table: "Catagories");

            migrationBuilder.DropForeignKey(
                name: "FK_CatagoryContact_Catagories_CategoriesId",
                table: "CatagoryContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Catagories",
                table: "Catagories");

            migrationBuilder.RenameTable(
                name: "Catagories",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_Catagories_AppUserId",
                table: "Categories",
                newName: "IX_Categories_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CatagoryContact_Categories_CategoriesId",
                table: "CatagoryContact",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_AppUserId",
                table: "Categories",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
