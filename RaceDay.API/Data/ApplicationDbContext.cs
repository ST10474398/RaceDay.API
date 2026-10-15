using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventEnrollment> EventEnrollments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<EventSponsor> EventSponsors { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---------------------------------------------------------
            // 1. USER (Organiser) -> EVENTS Relationship
            // ---------------------------------------------------------

            // Tell EF that these custom-named properties are the Primary Keys
            builder.Entity<EventEnrollment>().HasKey(en => en.EnrolmentID);
            builder.Entity<EventSponsor>().HasKey(es => es.EventSponsorID);

            builder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete events if user is deleted

            // ---------------------------------------------------------
            // 2. USER -> ENROLMENTS Relationship
            // ---------------------------------------------------------
            builder.Entity<EventEnrollment>()
                .HasOne(en => en.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // 3. CATEGORY -> EVENTS Relationship
            // ---------------------------------------------------------
            builder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // 4. EVENT -> ENROLMENTS Relationship
            // ---------------------------------------------------------
            builder.Entity<EventEnrollment>()
                .HasOne(en => en.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(en => en.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------------------------------------------
            // 5. ENROLMENT -> RESULT (One-to-One)
            // ---------------------------------------------------------
            builder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade); // If enrolment is deleted, delete result

            // ---------------------------------------------------------
            // 6. EVENT <-> SPONSOR (Many-to-Many through EventSponsor)
            // ---------------------------------------------------------
            builder.Entity<EventSponsor>()
                .HasOne(es => es.Event)
                .WithMany(e => e.EventSponsors)
                .HasForeignKey(es => es.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventSponsor>()
                .HasOne(es => es.Sponsor)
                .WithMany(s => s.EventSponsors)
                .HasForeignKey(es => es.SponsorID)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------------
            // 7. UNIQUE CONSTRAINTS
            // ---------------------------------------------------------
            // A user cannot enrol twice in the same event
            builder.Entity<EventEnrollment>()
                .HasIndex(en => new { en.UserID, en.EventID })
                .IsUnique();

            // A sponsor can only sponsor an event once
            builder.Entity<EventSponsor>()
                .HasIndex(es => new { es.EventID, es.SponsorID })
                .IsUnique();

            // Unique Email (Identity handles this automatically, but good to be explicit)
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}