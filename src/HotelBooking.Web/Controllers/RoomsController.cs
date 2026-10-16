using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hiển thị danh sách phòng
    public async Task<IActionResult> Index()
    {
        var rooms = await _context.Rooms
            .AsNoTracking()
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        return View(rooms);
    }

    // Hiển thị form thêm phòng
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Lưu phòng mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        ModelState.Remove(nameof(Room.Bookings));

        if (await _context.Rooms.AnyAsync(
            r => r.RoomNumber == room.RoomNumber))
        {
            ModelState.AddModelError(
                nameof(Room.RoomNumber),
                "Số phòng đã tồn tại.");
        }

        if (!ModelState.IsValid)
            return View(room);

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm phòng thành công!";
        return RedirectToAction(nameof(Index));
    }

    // Hiển thị form sửa phòng
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
            return NotFound();

        return View(room);
    }

    // Cập nhật phòng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Room room)
    {
        if (id != room.Id)
            return BadRequest();

        ModelState.Remove(nameof(Room.Bookings));

        if (await _context.Rooms.AnyAsync(
            r => r.RoomNumber == room.RoomNumber && r.Id != id))
        {
            ModelState.AddModelError(
                nameof(Room.RoomNumber),
                "Số phòng đã tồn tại.");
        }

        if (!ModelState.IsValid)
            return View(room);

        var existingRoom = await _context.Rooms.FindAsync(id);

        if (existingRoom == null)
            return NotFound();

        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.RoomType = room.RoomType;
        existingRoom.Price = room.Price;
        existingRoom.Status = room.Status;
        existingRoom.Description = room.Description;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật phòng thành công!";
        return RedirectToAction(nameof(Index));
    }

    // Xóa phòng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
            return NotFound();

        bool hasBookings = await _context.Bookings
            .AnyAsync(b => b.RoomId == id);

        if (hasBookings)
        {
            TempData["Error"] =
                "Không thể xóa phòng đã có lịch đặt.";
            return RedirectToAction(nameof(Index));
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xóa phòng thành công!";
        return RedirectToAction(nameof(Index));
    }
}