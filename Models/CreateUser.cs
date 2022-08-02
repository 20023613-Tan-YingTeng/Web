using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class CreateUser
    {

        public int UserId { get; set; }

        [Required(ErrorMessage = "Enter Username")]
        [Remote(action: "checkUniqueUsername", controller: "Account")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Enter Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Select Role ")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "Enter Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm Password Required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords Unmatched")]
        public string ConfirmPwd { get; set; } = null!;

        [Required(ErrorMessage = "Enter Email")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Enter ContactNo")]
        [RegularExpression(@"[89]\d{7}", ErrorMessage = "Invalid Contact Number")]
        public string ContactNo { get; set; } = null!;
    }
}