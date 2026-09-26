using Microsoft.EntityFrameworkCore;
using Timetable.Api.Domain;

namespace Timetable.Api.Data;

public class TimetableContext(DbContextOptions<TimetableContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TimetableEvent> TimetableEvents => Set<TimetableEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<TimetableEvent>(entity =>
        {
            entity.ToTable("timetable_events");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartUtc).HasColumnName("start_utc");
            entity.Property(e => e.EndUtc).HasColumnName("end_utc");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => new { e.UserId, e.StartUtc });
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
