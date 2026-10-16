
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    [Required]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Required]
    public DateTime CheckIn { get; set; }

    [Required]
    public DateTime CheckOut { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = "Booked";
}
