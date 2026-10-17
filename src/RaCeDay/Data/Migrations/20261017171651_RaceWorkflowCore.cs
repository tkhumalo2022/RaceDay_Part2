using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class RaceWorkflowCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClosingDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                    table.CheckConstraint("CK_Event_Dates", "[ClosingDate] <= [EventDate]");
                    table.CheckConstraint("CK_Event_Status", "[Status] IN ('Planned','Open','Closed','Completed','Cancelled')");
                    table.CheckConstraint("CK_Event_Type", "[EventType] IN ('Run','Walk','Cycle')");
                    table.ForeignKey(
                        name: "FK_Event_AspNetUsers_OrganiserId",
                        column: x => x.OrganiserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Race_Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race_Category", x => x.Id);
                    table.UniqueConstraint("AK_Race_Category_Id_EventId", x => new { x.Id, x.EventId });
                    table.CheckConstraint("CK_Category_Values", "[DistanceKm] > 0 AND [EntryFee] >= 0 AND [MaxParticipants] > 0");
                    table.ForeignKey(
                        name: "FK_Race_Category_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrolment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    RaceNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EnrolmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolment", x => x.Id);
                    table.CheckConstraint("CK_Enrolment_Status", "[Status] IN ('Registered','Completed','Withdrawn','Cancelled')");
                    table.ForeignKey(
                        name: "FK_Enrolment_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolment_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolment_Race_Category_CategoryId_EventId",
                        columns: x => new { x.CategoryId, x.EventId },
                        principalTable: "Race_Category",
                        principalColumns: new[] { "Id", "EventId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Race_Result",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    FinishTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    OverallPosition = table.Column<int>(type: "int", nullable: true),
                    ResultStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Race_Result", x => x.Id);
                    table.CheckConstraint("CK_Result_Position", "[OverallPosition] IS NULL OR [OverallPosition] > 0");
                    table.CheckConstraint("CK_Result_Status", "[ResultStatus] IN ('Finished','DNS','DNF','DSQ')");
                    table.CheckConstraint("CK_Result_Times", "[StartTime] IS NULL OR [FinishTime] IS NULL OR [FinishTime] >= [StartTime]");
                    table.ForeignKey(
                        name: "FK_Race_Result_Enrolment_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_CategoryId_EventId",
                table: "Enrolment",
                columns: new[] { "CategoryId", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_EventId",
                table: "Enrolment",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_ParticipantId_EventId",
                table: "Enrolment",
                columns: new[] { "ParticipantId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_RaceNumber",
                table: "Enrolment",
                column: "RaceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_OrganiserId",
                table: "Event",
                column: "OrganiserId");

            migrationBuilder.CreateIndex(
                name: "IX_Race_Category_EventId_CategoryName",
                table: "Race_Category",
                columns: new[] { "EventId", "CategoryName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Race_Result_EnrolmentId",
                table: "Race_Result",
                column: "EnrolmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Race_Result");

            migrationBuilder.DropTable(
                name: "Enrolment");

            migrationBuilder.DropTable(
                name: "Race_Category");

            migrationBuilder.DropTable(
                name: "Event");
        }
    }
}
