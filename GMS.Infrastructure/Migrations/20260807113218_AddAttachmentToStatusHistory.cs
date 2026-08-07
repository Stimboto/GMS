using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentToStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttachmentId",
                table: "StatusHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistories_AttachmentId",
                table: "StatusHistories",
                column: "AttachmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistories_Attachments_AttachmentId",
                table: "StatusHistories",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistories_Attachments_AttachmentId",
                table: "StatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_StatusHistories_AttachmentId",
                table: "StatusHistories");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "StatusHistories");
        }
    }
}
