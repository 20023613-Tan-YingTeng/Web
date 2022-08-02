using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace xShapes.Controllers;

public class AccountController : Controller
{
    private const string REDIRECT_CNTR = "Inventory";
    private const string REDIRECT_ACTN = "InventoryMain";
    private const string LOGIN_VIEW = "Login";

    private readonly AppDbContext _dbCtx;

    public AccountController(AppDbContext dbContext)
    {
        _dbCtx = dbContext;
    }

    private bool AuthenticateUser(string uid, string pw,
                                  out ClaimsPrincipal? principal)
    {
        principal = null;

        var sql = String.Format(
            @"SELECT * FROM Users 
               WHERE Username = '{0}' 
                 AND Password = HASHBYTES('SHA1', '{1}')",
            uid.EscQuote(),  // prevent SQL Injection 
            pw.EscQuote());  // prevent SQL Injection
        Users? appUser = _dbCtx.Users
            .FromSqlRaw(sql)
            .FirstOrDefault();

        if (appUser != null)
        {
            principal =
               new ClaimsPrincipal(
                  new ClaimsIdentity(
                     new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, appUser.Username),
                        new Claim(ClaimTypes.Name, appUser.Name),
                        new Claim(ClaimTypes.Role, appUser.Role)
                     },
                     "Basic"
                  )
               );
            return true;
        }
        return false;
    }

    //Return URL show which page to go to after login = null means go back to login page
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        TempData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Login";
        return View(LOGIN_VIEW);
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(LoginUser user)
    {
        if (!AuthenticateUser(user.Username, user.Password,
                out ClaimsPrincipal? principal))
        {
            ViewData["Message"] = "Incorrect Username or Password";
            ViewData["MsgType"] = "warning";
            return View(LOGIN_VIEW);
        }
        else
        {
            HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               principal!,
               new AuthenticationProperties
               {
                   IsPersistent = true
               });

            var updateSQL =
                @"UPDATE Users 
                    SET LastLogin = GETDATE() 
                    WHERE Username = '{0}'";
            string sql = String.Format(updateSQL, user.Username);
            int _ = _dbCtx.Database.ExecuteSqlRaw(sql);

            string? returnUrl = TempData["returnUrl"]?.ToString();
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
        }
    }

    [Authorize]
    public IActionResult Logoff(string? returnUrl = null)
    {
        HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
    }

    [AllowAnonymous]
    public IActionResult Forbidden()
    {
        return View();
    }

    [Authorize]
    public IActionResult CreateUser()
    {
        ViewData["title"] = "CreateUser";

        return View();
    }

    [Authorize]
    [HttpPost]
    public IActionResult CreateUser(CreateUser createUser)
    {
        if (ModelState.IsValid)
        {
            string apiKey = CreateMD5(createUser.Username);
            int rows_affected = _dbCtx.Database
                .ExecuteSqlInterpolated(
                    $@"INSERT INTO Users
                        (Username, Name, Role, Password, Email, ContactNo, ApiKey) 
                   VALUES ({createUser.Username}, {createUser.Name}, 
                        {createUser.Role},  HASHBYTES('SHA1', CONVERT(VARCHAR, {createUser.Password})),
                           {createUser.Email}, {createUser.ContactNo}, {apiKey})"
            );

            if (rows_affected == 1)
                TempData["Msg"] = "New user added!";
            else
                TempData["Msg"] = "Failed to update database!";
        }
        else
        {
            TempData["Msg"] = "Invalid information entered";
        }
        return RedirectToAction("UsersMain", "Users");
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes); // .NET 5 +

            // Convert the byte array to hexadecimal string prior to .NET 5
            // StringBuilder sb = new System.Text.StringBuilder();
            // for (int i = 0; i < hashBytes.Length; i++)
            // {
            //     sb.Append(hashBytes[i].ToString("X2"));
            // }
            // return sb.ToString();
        }
    }

    [Authorize]
    public IActionResult ChangePassword()
    {
        ViewData["title"] = "ChangePassword";
        return View();
    }

    [Authorize]
    [HttpPost]
    public IActionResult ChangePassword(PasswordDTO pwd)
    {
        var userid = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        if (_dbCtx.Database.ExecuteSqlInterpolated(
              $@"UPDATE Users 
                    SET Password = 
                        HASHBYTES('SHA1', CONVERT(VARCHAR, {pwd.NewPwd})) 
                  WHERE Username = {userid} 
                    AND Password = 
                         HASHBYTES('SHA1', CONVERT(VARCHAR, {pwd.CurrentPwd}))"
                ) == 1)
            ViewData["Msg"] = "Password Updated";
        else
            ViewData["Msg"] = "Failed to Update Password!";
        HttpContext.SignOutAsync(
             CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
    }

    [Authorize]
    public JsonResult VerifyCurrentPassword(string CurrentPwd)
    {
        var userid =
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Users? user = _dbCtx.Users
            .FromSqlInterpolated(
                $@"SELECT * FROM Users 
                    WHERE Username = {userid} 
                      AND Password = HASHBYTES('SHA1', 
                          CONVERT(VARCHAR, {CurrentPwd}))")
            .FirstOrDefault();

        if (user != null)          // User's Password Found
            return Json(true);     // Current Password Valid
        else
            return Json(false);    // Current Password Invalid
    }

    [Authorize]
    public JsonResult VerifyNewPassword(string NewPwd)
    {
        var userid =
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Users? user = _dbCtx.Users
            .FromSqlInterpolated(
                $@"SELECT * FROM Users 
                    WHERE Username = {userid} 
                      AND Password = HASHBYTES('SHA1', 
                          CONVERT(VARCHAR, {NewPwd}))")
            .FirstOrDefault();

        // New Password cannot be the same as Current Password
        if (user == null)        // User's Password Not Found
            return Json(true);   // New Password Valid
        else
            return Json(false);  // New Password Invalid
    }

    [Authorize]
    public JsonResult VerifyNewUsername(string NewUname)
    {
        DbSet<Users> dbs = _dbCtx.Users;

        Users? user = dbs
            .FromSqlInterpolated(
                $"SELECT * FROM Users WHERE Username = {NewUname}")
            .FirstOrDefault();

        if (user == null)
            return Json(true);
        else
            return Json(false);
    }

    public IActionResult ChangeUsername()
    {
        ViewData["title"] = "ChangeUsername";
        ViewData["userid"] =
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        return View();
    }

    [HttpPost]
    public IActionResult ChangeUsername(UsernameDTO uu)
    {
        var userid = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var theNewUsername = uu.NewUname;
        var newApiKey = CreateMD5(uu.NewUname);

        if (_dbCtx.Database.ExecuteSqlInterpolated(
            @$"UPDATE Users 
                  SET Username={theNewUsername} , ApiKey={newApiKey}
                WHERE Username={userid}") == 1)
            ViewData["Msg"] = "Username successfully updated!";
        else
            ViewData["Msg"] = "Failed to update username!";

        HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
    }

    // For remote model validation check
    /* Not Used 
    public IActionResult checkUsername(string Username)
    {
        Users? user = _dbCtx.Users.Where(i => i.Username.Equals(Username)).FirstOrDefault();
        if (user is null)
        {
            return Json("Incorrect Username.");
        }
        return Json(true);
    }
    */


    // For remote model validation check
    public IActionResult checkUniqueUsername(string Username)
    {
        Users? user = _dbCtx.Users.Where(i => i.Username.Equals(Username)).FirstOrDefault();
        if (user is not null)
        {
            return Json("Username is taken");
        }
        return Json(true);
    }


}
