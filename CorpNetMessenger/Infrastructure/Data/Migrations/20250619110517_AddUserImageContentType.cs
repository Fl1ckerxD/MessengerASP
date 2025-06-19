using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorpNetMessenger.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserImageContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "AspNetUsers");
        }
    }
}
