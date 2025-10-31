using Booking.System.ReservationService.DataBase.Models;
using Booking.System.ReservationService.DataBase.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.System.ReservationService.DataBase.Context;

public class ReservationContext(DbContextOptions<ReservationContext> options) : DbContext(options)
{
    public DbSet<DbHotel> Hotels { get; set; }
    public DbSet<DbReservation> Reservations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbHotel>(entity =>
        {
            entity.ToTable("hotel"); 
            
            entity.Property(p => p.Id)
                .HasColumnName("id"); 
            
            entity.Property(p => p.HotelUid)
                .HasColumnName("hotel_uid");
            
            entity.Property(p => p.Name)
                .HasColumnName("name");
            
            entity.Property(p => p.Country)
                .HasColumnName("country");
            
            entity.Property(p => p.City)
                .HasColumnName("city");
            
            entity.Property(p => p.Address)
                .HasColumnName("address");
            
            entity.Property(p => p.Stars)
                .HasColumnName("stars");
            
            entity.Property(p => p.Price)
                .HasColumnName("price");
        });

        modelBuilder.Entity<DbReservation>(entity =>
        {
            entity.ToTable("reservation"); 
        
            entity.Property(pp => pp.Id)
                .HasColumnName("id");
            
            entity.Property(pp => pp.ReservationUid)
                .HasColumnName("reservation_uid");
            
            entity.Property(pp => pp.Username)
                .HasColumnName("username");
            
            entity.Property(pp => pp.PaymentUid)
                .HasColumnName("payment_uid");
            
            entity.Property(pp => pp.HotelId)
                .HasColumnName("hotel_id");
            
            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasDefaultValue(DbPaymentStatus.PAID)
                .HasColumnName("status");
            
            entity.Property(pp => pp.StartDate)
                .HasColumnName("start_date");
            
            entity.Property(pp => pp.EndDate)
                .HasColumnName("end_date");
            
            entity.HasOne(r => r.Hotel)
                .WithMany(h => h.Reservations)
                .HasForeignKey(r => r.HotelId);
            
        });

        base.OnModelCreating(modelBuilder);
    }
}

