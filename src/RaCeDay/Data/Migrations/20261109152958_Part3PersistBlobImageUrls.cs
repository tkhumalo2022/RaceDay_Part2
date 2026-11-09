using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class Part3PersistBlobImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerBlobUrl",
                table: "Event",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileBlobUrl",
                table: "AspNetUsers",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerBlobUrl",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "ProfileBlobUrl",
                table: "AspNetUsers");
        }
    }
}
