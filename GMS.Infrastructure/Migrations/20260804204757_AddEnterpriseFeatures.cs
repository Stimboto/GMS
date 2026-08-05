using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "StatusHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackRemarks",
                table: "Grievances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SatisfactionRating",
                table: "Grievances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingId",
                table: "Grievances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "StatusHistories");

            migrationBuilder.DropColumn(
                name: "FeedbackRemarks",
                table: "Grievances");

            migrationBuilder.DropColumn(
                name: "SatisfactionRating",
                table: "Grievances");

            migrationBuilder.DropColumn(
                name: "TrackingId",
                table: "Grievances");
        }
    }
}
