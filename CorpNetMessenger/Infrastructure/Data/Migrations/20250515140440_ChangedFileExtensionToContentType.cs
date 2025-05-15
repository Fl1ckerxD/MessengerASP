using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorpNetMessenger.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFileExtensionToContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "Attachments");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.Sql(@"
                CREATE TABLE Attachments (
                    Id UNIQUEIDENTIFIER PRIMARY KEY ROWGUIDCOL NOT NULL DEFAULT NEWID(),
                    MessageId nvarchar(450) REFERENCES Messages (Id) NOT NULL,
                    FileName NVARCHAR(255) NOT NULL,
                    FileLength bigint NOT NULL,
                    ContentType nvarchar(max) NOT NULL,
                    FileData VARBINARY(MAX) FILESTREAM NOT NULL
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Attachments");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "Attachments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
