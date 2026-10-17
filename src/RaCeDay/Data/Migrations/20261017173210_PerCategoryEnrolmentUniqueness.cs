using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerCategoryEnrolmentUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrolment_ParticipantId_EventId",
                table: "Enrolment");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_ParticipantId_CategoryId",
                table: "Enrolment",
                columns: new[] { "ParticipantId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrolment_ParticipantId_CategoryId",
                table: "Enrolment");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_ParticipantId_EventId",
                table: "Enrolment",
                columns: new[] { "ParticipantId", "EventId" },
                unique: true);
        }
    }
}
