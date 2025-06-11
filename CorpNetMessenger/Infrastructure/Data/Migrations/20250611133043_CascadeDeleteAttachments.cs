using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CorpNetMessenger.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Attachmen__Messa__2DE6D218",
                table: "Attachments");

            migrationBuilder.AddForeignKey(
                name: "FK__Attachmen__Messa__2DE6D218",
                table: "Attachments",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
               name: "FK__Attachmen__Messa__2DE6D218",
               table: "Attachments");

            migrationBuilder.AddForeignKey(
                name: "FK__Attachmen__Messa__2DE6D218",
                table: "Attachments",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
