using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Models;

namespace RaCeDay.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<RaceCategory> Categories => Set<RaceCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();
    public DbSet<PaymentAttempt> Payments => Set<PaymentAttempt>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<RaceEvent>(e =>
        {
            e.ToTable("Event", t =>
            {
                t.HasCheckConstraint("CK_Event_Dates", "[ClosingDate] <= [EventDate]");
                t.HasCheckConstraint("CK_Event_Type", "[EventType] IN ('Run','Walk','Cycle')");
                t.HasCheckConstraint("CK_Event_Status", "[Status] IN ('Planned','Open','Closed','Completed','Cancelled')");
            });
            e.Property(x => x.EventName).HasMaxLength(150);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.EventType).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<RaceCategory>(e =>
        {
            e.ToTable("Race_Category", t =>
            {
                t.HasCheckConstraint("CK_Category_Values", "[DistanceKm] > 0 AND [EntryFee] >= 0 AND [MaxParticipants] > 0");
            });
            e.Property(x => x.CategoryName).HasMaxLength(100);
            e.Property(x => x.DistanceKm).HasPrecision(6, 2);
            e.Property(x => x.EntryFee).HasPrecision(10, 2);
            e.HasIndex(x => new { x.EventId, x.CategoryName }).IsUnique();
            e.HasAlternateKey(x => new { x.Id, x.EventId });
            e.HasOne<RaceEvent>().WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Enrolment>(e =>
        {
            e.ToTable("Enrolment", t => t.HasCheckConstraint("CK_Enrolment_Status", "[Status] IN ('Registered','Completed','Withdrawn','Cancelled')"));
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.RaceNumber).HasMaxLength(32);
            e.HasIndex(x => x.RaceNumber).IsUnique();
            e.HasIndex(x => new { x.ParticipantId, x.CategoryId }).IsUnique();
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RaceEvent>().WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RaceCategory>().WithMany().HasForeignKey(x => new { x.CategoryId, x.EventId })
                .HasPrincipalKey(x => new { x.Id, x.EventId }).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<PaymentAttempt>(e =>
        {
            e.ToTable("Payment", t =>
            {
                t.HasCheckConstraint("CK_Payment_Amount", "[Amount] >= 0");
                t.HasCheckConstraint("CK_Payment_Method", "[PaymentMethod] IN ('Card','EFT','Cash','Other')");
                t.HasCheckConstraint("CK_Payment_Status", "[Status] IN ('Pending','Paid','Failed','Refunded')");
            });
            e.Property(x => x.Amount).HasPrecision(10, 2);
            e.Property(x => x.PaymentMethod).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.TransactionReference).HasMaxLength(80);
            e.HasIndex(x => x.TransactionReference).IsUnique().HasFilter("[TransactionReference] IS NOT NULL");
            e.HasIndex(x => x.EnrolmentId).IsUnique().HasFilter("[Status] = 'Paid'");
            e.HasOne<Enrolment>().WithMany().HasForeignKey(x => x.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<RaceResult>(e =>
        {
            e.ToTable("Race_Result", t =>
            {
                t.HasCheckConstraint("CK_Result_Position", "[OverallPosition] IS NULL OR [OverallPosition] > 0");
                t.HasCheckConstraint("CK_Result_Times", "[StartTime] IS NULL OR [FinishTime] IS NULL OR [FinishTime] >= [StartTime]");
                t.HasCheckConstraint("CK_Result_Status", "[ResultStatus] IN ('Finished','DNS','DNF','DSQ')");
            });
            e.Property(x => x.ResultStatus).HasMaxLength(20);
            e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.HasOne<Enrolment>().WithMany().HasForeignKey(x => x.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
