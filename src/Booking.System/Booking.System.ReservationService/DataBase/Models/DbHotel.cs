using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.System.ReservationService.DataBase.Models;

public class DbHotel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public Guid HotelUid { get; set; }
    
    public string Name { get; set; }
    
    public string Country { get; set; }
    
    public string City { get; set; }
    
    public string Address { get; set; }
    
    public int Stars { get; set; }
    
    public int Price { get; set; }
    
    public virtual ICollection<DbReservation> Reservations { get; set; } = new List<DbReservation>();

    public DbHotel(int id,
        Guid hotelUid,
        string name,
        string country,
        string city,
        string address,
        int stars,
        int price)
    {
        Id = id;
        HotelUid = hotelUid;
        Name = name;
        Country = country;
        City = city;
        Address = address;
        Stars = stars;
        Price = price;
    }
}