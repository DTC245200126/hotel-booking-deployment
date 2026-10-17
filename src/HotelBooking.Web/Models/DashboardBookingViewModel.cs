namespace HotelBooking.Web.Models;

public class DashboardBookingViewModel
{
    public int Id { get; init; }

    public string CustomerName { get; init; } = "";

    public string RoomNumber { get; init; } = "";

    public DateTime CheckIn { get; init; }

    public DateTime CheckOut { get; init; }

    public decimal TotalPrice { get; init; }

    public string Status { get; init; } = "";
}
