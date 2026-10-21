using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.CheckConstraint("CK_Payment_Amount", "[Amount] >= 0");
                    table.CheckConstraint("CK_Payment_Method", "[PaymentMethod] IN ('Card','EFT','Cash','Other')");
                    table.CheckConstraint("CK_Payment_Status", "[Status] IN ('Pending','Paid','Failed','Refunded')");
                    table.ForeignKey(
                        name: "FK_Payment_Enrolment_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_EnrolmentId",
                table: "Payment",
                column: "EnrolmentId",
                unique: true,
                filter: "[Status] = 'Paid'");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TransactionReference",
                table: "Payment",
                column: "TransactionReference",
                unique: true,
                filter: "[TransactionReference] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment");
        }
    }
}
