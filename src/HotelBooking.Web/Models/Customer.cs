using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Customer
{
    public int Id { get; set; }

    // Họ và tên khách hàng
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100)]
    public string FullName { get; set; } = "";

    // Số điện thoại Việt Nam
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(
        @"^0[35789][0-9]{8}$",
        ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 03, 05, 07, 08 hoặc 09.")]
    public string PhoneNumber { get; set; } = "";

    // Email không bắt buộc
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string? Email { get; set; }

    // Danh sách đặt phòng
    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}