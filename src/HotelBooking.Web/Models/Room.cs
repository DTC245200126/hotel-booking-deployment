
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string RoomType { get; set; } = "";

    [Range(1, 100000000)]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Available";

    public string? Description { get; set; }

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}
