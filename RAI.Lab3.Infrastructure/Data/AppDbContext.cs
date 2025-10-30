using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Roles;
using RAI.Lab3.Infrastructure.Security;

namespace RAI.Lab3.Infrastructure.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> dbContextOptions,
    ICurrentUserService currentUserService) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(dbContextOptions)
{
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    private const string ExclusionSql =
        """
        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'reservations_no_overlap_per_availability'
          ) THEN
            ALTER TABLE public.reservations
              ADD CONSTRAINT reservations_no_overlap_per_availability
              EXCLUDE USING gist (
                teacher_availability_id WITH =,
                period                  WITH &&
              );
          END IF;
        END$$;

        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'reservations_period_not_empty'
          ) THEN
            ALTER TABLE public.reservations
              ADD CONSTRAINT reservations_period_not_empty
              CHECK (NOT isempty(period));
          END IF;
        END$$;

        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'ta_no_overlap_per_teacher_room'
          ) THEN
            ALTER TABLE public.teacher_availabilities
              ADD CONSTRAINT ta_no_overlap_per_teacher_room
              EXCLUDE USING gist (
                teacher_id WITH =,
                room_id    WITH =,
                periods    WITH &&
              );
          END IF;
        END$$;

        DO $$
        BEGIN
          IF NOT EXISTS (
            SELECT 1 FROM pg_constraint WHERE conname = 'reservations_student_no_overlap'
          ) THEN
            ALTER TABLE public.reservations
              ADD CONSTRAINT reservations_student_no_overlap
              EXCLUDE USING gist (
                student_id WITH =,
                period     WITH &&
              )
              WHERE (student_id IS NOT NULL);
          END IF;
        END$$;
        """;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("btree_gist");

        builder.Entity<Room>(eb =>
        {
            eb.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            eb.Property(r => r.Number)
                .IsRequired();

            eb.HasIndex(r => r.Number)
                .IsUnique();

            eb.HasIndex(r => r.Name)
                .IsUnique();
        });

        builder.Entity<Reservation>(eb =>
        {
            // 1:N relationship between Reservation and User (Student)
            eb.HasOne(r => r.Student)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TeacherAvailability>(eb =>
        {
            // 1:N relationship between TeacherAvailability and User (Teacher)
            eb.HasOne(ta => ta.Teacher)
                .WithMany(u => u.Availabilities)
                .HasForeignKey(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:N relationship between TeacherAvailability and Room
            eb.HasOne(ta => ta.Room)
                .WithMany(r => r.TeacherAvailabilities)
                .HasForeignKey(ta => ta.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // N:1 relationship between TeacherAvailability and Reservation
            eb.HasMany(ta => ta.Reservations)
                .WithOne(r => r.TeacherAvailability)
                .HasForeignKey(r => r.TeacherAvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QFs
        builder.Entity<TeacherAvailability>()
            .HasQueryFilter(ta =>
                ta.TeacherId == currentUserService.UserId && currentUserService.UserRole == AppRoles.Teacher);

        builder.Entity<Reservation>()
            .HasQueryFilter(r =>
                r.StudentId == currentUserService.UserId ||
                r.TeacherAvailability.TeacherId == currentUserService.UserId);
    }

    public async Task EnsureConstraintsCreatedAsync(CancellationToken cancellationToken = default) =>
        await Database.ExecuteSqlRawAsync(ExclusionSql, cancellationToken);
}