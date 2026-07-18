using BusTicketReservationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Bus> Buses { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<BusSchedule> BusSchedules { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Route
            modelBuilder.Entity<Route>(b =>
            {
                b.Property(r => r.BoardingPoint).HasMaxLength(200).IsRequired(false);
                b.Property(r => r.DroppingPoint).HasMaxLength(200).IsRequired(false);
            });

            // Passenger
            modelBuilder.Entity<Passenger>(b =>
            {
                b.Property(p => p.BoardingPoint).HasMaxLength(200).IsRequired(false);
                b.Property(p => p.DroppingPoint).HasMaxLength(200).IsRequired(false);
            });

            // Ticket
            modelBuilder.Entity<Ticket>(b =>
            {
                b.Property(t => t.BoardingPoint).HasMaxLength(200).IsRequired(false);
                b.Property(t => t.DroppingPoint).HasMaxLength(200).IsRequired(false);
                b.HasIndex(t => new { t.SeatId, t.BusScheduleId }).IsUnique();
            });

            // Relationships
            modelBuilder.Entity<Bus>()
                .HasMany(b => b.Seats)
                .WithOne(s => s.Bus)
                .HasForeignKey(s => s.BusId);

            modelBuilder.Entity<Bus>()
                .HasMany(b => b.Schedules)
                .WithOne(s => s.Bus)
                .HasForeignKey(s => s.BusId);

            modelBuilder.Entity<Route>()
                .HasMany(r => r.BusSchedules)
                .WithOne(s => s.Route)
                .HasForeignKey(s => s.RouteId);

            modelBuilder.Entity<BusSchedule>()
                .HasMany(bs => bs.Tickets)
                .WithOne(t => t.BusSchedule)
                .HasForeignKey(t => t.BusScheduleId);

            modelBuilder.Entity<Seat>()
                .HasMany(s => s.Tickets)
                .WithOne(t => t.Seat)
                .HasForeignKey(t => t.SeatId);

            modelBuilder.Entity<Passenger>()
                .HasMany(p => p.Tickets)
                .WithOne(t => t.Passenger)
                .HasForeignKey(t => t.PassengerId);
        }
    }

}
