using Microsoft.AspNetCore.Mvc;

namespace xShapes.Models;

public class UsernameDTO
{
    public string CurrentUsername { get; set; } = null!;

    [Required(ErrorMessage = "New Username Required")]
    [DataType(DataType.Text)]
    [Remote("VerifyNewUsername", "Account", 
            ErrorMessage = "Username Not Available")]
    public string NewUname { get; set; } = null!;

    [Required(ErrorMessage = "Confirm Username Required")]
    [DataType(DataType.Text)]
    [Compare("NewUname", ErrorMessage = "Usernames not matched")]
    public string ConfirmUname { get; set; } = null!;
}
