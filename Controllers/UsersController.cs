using Microsoft.AspNetCore.Mvc;

namespace xShapes.Controllers
{
    public class UsersController : Controller
    {

        private readonly AppDbContext _dbCtx;

        public UsersController(AppDbContext dbContext)
        {
            _dbCtx = dbContext;
        }
        public IActionResult UsersMain()
        {
            ViewData["title"] = "Users";
            ViewData["name"] = User.FindFirst(ClaimTypes.Name)!.Value;
            ViewData["row"] = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Users> model = null!;
            Users user = _dbCtx.Users.First();
            model = _dbCtx.Users
               .ToList();

            return View(model);
        }

        public IActionResult Update(int id)
        {
            ViewData["title"] = "Update";
            Users user = _dbCtx.Users
                .Where(i => i.UserId == id)
                .First();
            return View(user);
        }

        [HttpPost]
        public IActionResult Update(Users user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Msg"] = "Invalid Information Entered";
                return RedirectToAction("UsersMain");
            }

            Users? updatedUser = _dbCtx.Users
                .Where(i => i.UserId == user.UserId)
                .First();

            if (updatedUser == null)
            {
                TempData["Msg"] = "User Not Found";
            }
            else
            {

                // Replace current in database with new value in the list
                updatedUser.Name = user.Name;
                updatedUser.Role = user.Role;
                updatedUser.Email = user.Email;
                updatedUser.ContactNo = user.ContactNo;

                TempData["Msg"] = _dbCtx.SaveChanges() switch
                {
                    0 => "No Changes Made",
                    1 => "Inventory Item Updated",
                    _ => "Unexpected Database Error"
                };
            }
            return RedirectToAction("UsersMain");
        }
        public IActionResult Delete(int id)
        {
            DbSet<Users> dbs = _dbCtx.Users;
            Users? user = dbs
                .Where(i => i.UserId == id)
                .FirstOrDefault();

            if (user == null)
            {
                TempData["Msg"] = "User Not Found";
                return RedirectToAction("UsersMain");
            }


            List<Inventory> itemList = _dbCtx.Inventory
                .Where(i => i.User.Contains(user)).ToList();
            if (itemList.Count != 0)
            {
                TempData["Msg"] = "Users is being assigned to an Inventory";
                return RedirectToAction("UsersMain");
            }
            dbs.Remove(user);
            TempData["Msg"] = _dbCtx.SaveChanges() switch
            {
                1 => "User Deleted",
                _ => "Unexpected Database Error"
            };

            return RedirectToAction("UsersMain");
        }
    }
}
