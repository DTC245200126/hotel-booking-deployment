using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Controllers;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách đặt phòng
    public async Task<IActionResult> Index()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.Customer)
            .AsNoTracking()
            .OrderByDescending(b => b.CheckIn)
            .ToListAsync();

        return View(bookings);
    }

    // Danh sách phòng
    private async Task LoadRooms()
    {
        var rooms = await _context.Rooms
            .Where(r => r.Status == "Available")
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        ViewBag.Rooms = new SelectList(
            rooms.Select(r => new
            {
                r.Id,
                Display = $"Phòng {r.RoomNumber} - {r.RoomType} - {r.Price:N0} VNĐ/đêm"
            }),
            "Id",
            "Display"
        );
    }

    // Hiển thị form đặt phòng
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadRooms();

        return View(new Booking
        {
            CheckIn = DateTime.Today,
            CheckOut = DateTime.Today.AddDays(1)
        });
    }

    // Xử lý đặt phòng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("RoomId,CheckIn,CheckOut")] Booking booking,
        string FullName,
        string PhoneNumber,
        string? Email)
    {
        ModelState.Remove(nameof(Booking.Room));
        ModelState.Remove(nameof(Booking.Customer));
        ModelState.Remove(nameof(Booking.CustomerId));

        ViewBag.FullName = FullName;
        ViewBag.PhoneNumber = PhoneNumber;
        ViewBag.Email = Email;

        // Kiểm tra thông tin khách hàng
        var customer = new Customer
        {
            FullName = FullName?.Trim() ?? "",
            PhoneNumber = PhoneNumber?.Trim() ?? "",
            Email = string.IsNullOrWhiteSpace(Email)
                ? null
                : Email.Trim()
        };

        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(
            customer,
            new ValidationContext(customer),
            validationResults,
            true))
        {
            foreach (var result in validationResults)
            {
                ModelState.AddModelError(
                    "",
                    result.ErrorMessage ?? "Thông tin khách hàng không hợp lệ.");
            }
        }

        // Chuẩn hóa ngày sang UTC
        booking.CheckIn = DateTime.SpecifyKind(
            booking.CheckIn.Date,
            DateTimeKind.Utc);

        booking.CheckOut = DateTime.SpecifyKind(
            booking.CheckOut.Date,
            DateTimeKind.Utc);

        // Kiểm tra ngày
        if (booking.CheckOut <= booking.CheckIn)
        {
            ModelState.AddModelError(
                nameof(Booking.CheckOut),
                "Ngày trả phòng phải sau ngày nhận phòng.");
        }

        // Kiểm tra phòng
        var room = await _context.Rooms.FindAsync(booking.RoomId);

        if (room == null || room.Status != "Available")
        {
            ModelState.AddModelError(
                nameof(Booking.RoomId),
                "Phòng không tồn tại hoặc không khả dụng.");
        }

        // Kiểm tra trùng lịch
        if (room != null && booking.CheckOut > booking.CheckIn)
        {
            bool hasConflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == booking.RoomId &&
                b.Status != "Cancelled" &&
                booking.CheckIn < b.CheckOut &&
                booking.CheckOut > b.CheckIn);

            if (hasConflict)
            {
                ModelState.AddModelError(
                    nameof(Booking.RoomId),
                    "Phòng đã có lịch đặt trong khoảng thời gian này.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadRooms();
            return View(booking);
        }

        // Tính tổng tiền
        int nights = (booking.CheckOut - booking.CheckIn).Days;

        booking.TotalPrice = room!.Price * nights;
        booking.Status = "Booked";

        // Lưu khách hàng và đặt phòng trong cùng giao dịch
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        booking.CustomerId = customer.Id;

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        TempData["Success"] = "Đặt phòng thành công!";

        return RedirectToAction(nameof(Index));
    }
}