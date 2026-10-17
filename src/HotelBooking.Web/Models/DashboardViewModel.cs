namespace HotelBooking.Web.Models;

public class DashboardViewModel
{
    public int TotalRooms { get; init; }

    public int AvailableRooms { get; init; }

    public int OccupiedRooms { get; init; }

    public int UnavailableRooms { get; init; }

    public int TotalCustomers { get; init; }

    public int TotalBookings { get; init; }

    public List<DashboardBookingViewModel> RecentBookings { get; init; } = [];
}
