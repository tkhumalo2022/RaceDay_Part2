using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaCeDay.Data.Migrations
{
    /// <inheritdoc />
    public partial class Part1CategoryCountAndPaymentRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentParticipants",
                table: "Race_Category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_CurrentParticipants",
                table: "Race_Category",
                sql: "[CurrentParticipants] >= 0 AND [CurrentParticipants] <= [MaxParticipants]");

            // The Part 1 design stores the number of active registrations on each category.
            // Backfill legacy rows before enabling the SQL Server trigger.
            migrationBuilder.Sql("""
                UPDATE c
                SET [CurrentParticipants] = (
                    SELECT COUNT(*)
                    FROM dbo.[Enrolment] e
                    WHERE e.[CategoryId] = c.[Id] AND e.[Status] = 'Registered'
                )
                FROM dbo.[Race_Category] c;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER dbo.trg_Enrolment_UpdateCategoryCount
                ON dbo.[Enrolment]
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    ;WITH ChangedCategories AS
                    (
                        SELECT CategoryId FROM inserted
                        UNION
                        SELECT CategoryId FROM deleted
                    )
                    UPDATE c
                    SET CurrentParticipants = (
                        SELECT COUNT(*)
                        FROM dbo.[Enrolment] e
                        WHERE e.CategoryId = c.Id AND e.Status = 'Registered'
                    )
                    FROM dbo.[Race_Category] c
                    INNER JOIN ChangedCategories changes ON changes.CategoryId = c.Id;
                END;
                """);

            // The API checks payment amounts too; this independently protects direct SQL writes.
            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER dbo.trg_Payment_ValidateAmount
                ON dbo.[Payment]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (
                        SELECT 1 FROM inserted p
                        INNER JOIN dbo.[Enrolment] e ON e.Id = p.EnrolmentId
                        INNER JOIN dbo.[Race_Category] c ON c.Id = e.CategoryId
                        WHERE p.Amount <> c.EntryFee
                    )
                    BEGIN
                        THROW 51000, 'Payment amount must match the race category entry fee.', 1;
                    END
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_Payment_ValidateAmount;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo.trg_Enrolment_UpdateCategoryCount;");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_CurrentParticipants",
                table: "Race_Category");

            migrationBuilder.DropColumn(
                name: "CurrentParticipants",
                table: "Race_Category");
        }
    }
}
