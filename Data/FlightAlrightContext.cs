using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FlightAlright.Models;

namespace FlightAlright.Data
{
    public class FlightAlrightContext : DbContext
    {
        public FlightAlrightContext (DbContextOptions<FlightAlrightContext> options)
            : base(options)
        {
        }

        public DbSet<FlightAlright.Models.Employee> Employee { get; set; } = default!;
        public DbSet<FlightAlright.Models.Account> Account { get; set; } = default!;
        public DbSet<FlightAlright.Models.Airport> Airport { get; set; } = default!;
        public DbSet<FlightAlright.Models.Brand> Brand { get; set; } = default!;
        public DbSet<FlightAlright.Models.Class> Class { get; set; } = default!;
        public DbSet<FlightAlright.Models.Crew> Crew { get; set; } = default!;
        public DbSet<FlightAlright.Models.Flight> Flight { get; set; } = default!;
        public DbSet<FlightAlright.Models.Paycheck> Paycheck { get; set; } = default!;
        public DbSet<FlightAlright.Models.Plane> Plane { get; set; } = default!;
        public DbSet<FlightAlright.Models.Position> Position { get; set; } = default!;
        public DbSet<FlightAlright.Models.Price> Price { get; set; } = default!;
        public DbSet<FlightAlright.Models.Role> Role { get; set; } = default!;
        public DbSet<FlightAlright.Models.Ticket> Ticket { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
         .SelectMany(t => t.GetProperties())
         .Where(p => p.ClrType == typeof(string)))
            {
                property.SetColumnType("TEXT"); // Ustaw TEXT dla wszystkich stringów
            }

            base.OnModelCreating(modelBuilder);

            //Account - Role
            modelBuilder.Entity<Account>()
                .HasOne(r => r.Role)
                .WithMany()
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            //Class - Brand
            modelBuilder.Entity<Class>()
                .HasOne(r => r.Brand)
                .WithMany()
                .HasForeignKey(r => r.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            //Crew - Employee
            modelBuilder.Entity<Crew>()
                .HasOne(r => r.Employee)
                .WithMany()
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            //Crew - Flight
            modelBuilder.Entity<Crew>()
                .HasOne(r => r.Flight)
                .WithMany()
                .HasForeignKey(r => r.FlightId)
                .OnDelete(DeleteBehavior.SetNull);

            //Employee - Account
            modelBuilder.Entity<Employee>()
                .HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            //Employee - Position
            modelBuilder.Entity<Employee>()
                .HasOne(r => r.Position)
                .WithMany()
                .HasForeignKey(r => r.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            //Flight - DepartureAirport
            modelBuilder.Entity<Flight>()
                .HasOne(r => r.DepartureAirport)
                .WithMany()
                .HasForeignKey(r => r.DepartureAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            //Flight - ArrivalAirport
            modelBuilder.Entity<Flight>()
                .HasOne(r => r.ArrivalAirport)
                .WithMany()
                .HasForeignKey(r => r.ArrivalAirportId)
                .OnDelete(DeleteBehavior.SetNull);

            //Flight - Plane
            modelBuilder.Entity<Flight>()
                .HasOne(r => r.Plane)
                .WithMany()
                .HasForeignKey(r => r.PlaneId)
                .OnDelete(DeleteBehavior.SetNull);

            //Plane - Brand
            modelBuilder.Entity<Plane>()
                .HasOne(r => r.Brand)
                .WithMany()
                .HasForeignKey(r => r.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            //Price - Class
            modelBuilder.Entity<Price>()
                .HasOne(r => r.Class)
                .WithMany()
                .HasForeignKey(r => r.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            //Price - Flight
            modelBuilder.Entity<Price>()
                .HasOne(r => r.Flight)
                .WithMany()
                .HasForeignKey(r => r.FlightId)
                .OnDelete(DeleteBehavior.SetNull);

            //Ticket - Account
            modelBuilder.Entity<Ticket>()
                .HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            //Ticket - Price
            modelBuilder.Entity<Ticket>()
                .HasOne(r => r.Price)
                .WithMany()
                .HasForeignKey(r => r.PriceId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
