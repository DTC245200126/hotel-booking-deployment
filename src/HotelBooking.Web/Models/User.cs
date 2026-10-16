
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = "Admin";
}
