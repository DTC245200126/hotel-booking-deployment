using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBooking.Web.Data;
using HotelBooking.Web.Models;

namespace HotelBooking.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalRooms = await _context.Rooms.CountAsync();
        var availableRooms = await _context.Rooms
            .CountAsync(room => room.Status == "Available");
        var occupiedRooms = await _context.Rooms
            .CountAsync(room => room.Status == "Occupied");

        var overview = new DashboardViewModel
        {
            TotalRooms = totalRooms,
            AvailableRooms = availableRooms,
            OccupiedRooms = occupiedRooms,
            UnavailableRooms = totalRooms - availableRooms - occupiedRooms,
            TotalCustomers = await _context.Customers.CountAsync(),
            TotalBookings = await _context.Bookings.CountAsync(),
            RecentBookings = await _context.Bookings
                .OrderByDescending(booking => booking.Id)
                .Take(5)
                .Select(booking => new DashboardBookingViewModel
                {
                    Id = booking.Id,
                    CustomerName = booking.Customer.FullName,
                    RoomNumber = booking.Room.RoomNumber,
                    CheckIn = booking.CheckIn,
                    CheckOut = booking.CheckOut,
                    TotalPrice = booking.TotalPrice,
                    Status = booking.Status
                })
                .ToListAsync()
        };

        return View(overview);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
