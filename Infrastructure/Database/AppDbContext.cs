using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Genre)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.ReleaseDate).IsRequired();
            entity.Property(e => e.Language).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Actors).HasMaxLength(500);
            entity.Property(e => e.TrailerUrl).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NumberOfSeats).IsRequired();
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Screening>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.StartDateTime).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();

            entity.HasOne(s => s.Movie)
                  .WithMany()
                  .HasForeignKey(s => s.MovieId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(s => s.Hall)
                  .WithMany()
                  .HasForeignKey(s => s.HallId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasIndex(s => new { s.HallId, s.StartDateTime }).IsUnique();
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SeatNo).IsRequired();
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .IsRequired()
                  .HasMaxLength(32);
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(8);
            entity.Property(e => e.ExpiresAt).IsRequired();

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasOne(r => r.Screening)
                  .WithMany()
                  .HasForeignKey(r => r.ScreeningId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();

            entity.HasIndex(e => new { e.ScreeningId, e.SeatNo })
                  .IsUnique()
                  .HasFilter("[Status] IN (N'PENDING_PAYMENT', N'CONFIRMED')")
                  .HasDatabaseName("IX_Reservations_ScreeningId_SeatNo_Active");
        });
    }
}
