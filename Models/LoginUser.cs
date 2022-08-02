using Microsoft.AspNetCore.Mvc;

namespace xShapes.Models;

public class LoginUser
{
    [Required(ErrorMessage = "Enter Username")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Enter Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

}
