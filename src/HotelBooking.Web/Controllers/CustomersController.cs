using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách khách hàng
    public async Task<IActionResult> Index()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync();

        return View(customers);
    }

    // Form thêm khách hàng
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Lưu khách hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FullName,PhoneNumber,Email")] Customer customer)
    {
        if (!ModelState.IsValid)
            return View(customer);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm khách hàng thành công!";
        return RedirectToAction(nameof(Index));
    }

    // Form sửa khách hàng
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        return View(customer);
    }

    // Cập nhật khách hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,FullName,PhoneNumber,Email")] Customer customer)
    {
        if (id != customer.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(customer);

        var existing = await _context.Customers.FindAsync(id);

        if (existing == null)
            return NotFound();

        existing.FullName = customer.FullName;
        existing.PhoneNumber = customer.PhoneNumber;
        existing.Email = customer.Email;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật khách hàng thành công!";
        return RedirectToAction(nameof(Index));
    }

    // Xóa khách hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        bool hasBookings = await _context.Bookings
            .AnyAsync(b => b.CustomerId == id);

        if (hasBookings)
        {
            TempData["Error"] =
                "Không thể xóa khách hàng đã có lịch đặt phòng.";

            return RedirectToAction(nameof(Index));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Xóa khách hàng thành công!";
        return RedirectToAction(nameof(Index));
    }
}