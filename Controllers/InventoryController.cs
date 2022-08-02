using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace xShapes.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _dbCtx;

        public InventoryController(AppDbContext dbContext)
        {
            _dbCtx = dbContext;
        }
        [Authorize]
        public IActionResult InventoryMain()
        {
            ViewData["title"] = "Inventory";
            ViewData["name"] = User.FindFirst(ClaimTypes.Name)!.Value;
            ViewData["row"] = User.FindFirst(ClaimTypes.Role)!.Value;

            // If Admin, will pass the entire inventory items over
            // If User, pass only inventory items that belongs to the User
            List<InventoryItems> model = null!;
            if (User.IsInRole("Admin"))
            {
                model = _dbCtx.InventoryItems
                   .Include(i => i.Shape)
                   .Include(i => i.Product)
                   .Include(i => i.Inventory)
                   .ThenInclude(i => i.User)
                   .ToList();
            }

            else if (User.IsInRole("User"))
            {
                Users user = _dbCtx.Users.Where(i => i.Username.Equals(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)).First();
                model = _dbCtx.InventoryItems
                   .Include(i => i.Shape)
                   .Include(i => i.Product)
                   .Include(i => i.Inventory)
                   .ThenInclude(i => i.User)
                   .Where(i => i.Inventory.User.Contains(user))
                   .ToList();

                
            }
            return View(model);

        }
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id)
        {
            ViewData["title"] = "Update";
            InventoryItems inventory = _dbCtx.InventoryItems
                .Where(i => i.ItemId == id)
                .Include(i => i.Inventory)
                .ThenInclude(i => i.User)
                .First();

            // SelectList is to pass the <option> for Drop down list in view
            ViewData["shapes"] = new SelectList(
                _dbCtx.Shape.ToList(), "ShapeId", "Name");

            ViewData["products"] = new SelectList(
                _dbCtx.Product.ToList(), "ProductId", "Name");

            ViewData["users"] = _dbCtx.Users.ToList();

            // To have the names displayed as Name1, Name2 in "Assigned To:" drop down check box
            string names = "";
            foreach (Users user in inventory.Inventory.User)
            {
                names += user.Name + ", ";
            }
            names = names.Substring(0, names.Length - 2);
            ViewData["name"] = names;
            return View(inventory);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Update(InventoryItems items, string names)
        {
            if (!ModelState.IsValid)
            {
                TempData["Msg"] = "Invalid Information Entered";
                return RedirectToAction("InventoryMain");
            }

            InventoryItems? inventory = _dbCtx.InventoryItems
                .Where(i => i.ItemId == items.ItemId)
                .Include(i => i.Inventory)
                .ThenInclude(i => i.User)
                .First();

            if (inventory == null)
            {
                TempData["Msg"] = "Inventory Item Not Found";
            }
            else
            {
                // Store the names selected in a list of Users
                List<Users> userList = new List<Users>();
                string[] nameList = names.Split(", ");
                foreach (string name in nameList)
                {
                    // To retrieve user based on its Name
                    Users? user = _dbCtx.Users.Where(i => i.Name.Equals(name)).FirstOrDefault();
                    if (user is not null)
                    {
                        // If user is selected, will add it into the list
                        userList.Add(user);
                    }
                }

                // Replace current in database with new value in the list
                inventory.Inventory.User = userList;
                inventory.ProductId = items.ProductId;
                inventory.ShapeId = items.ShapeId;
                inventory.MinQuantity = items.MinQuantity;

                TempData["Msg"] = _dbCtx.SaveChanges() switch
                {
                    0 => "No Changes Made",
                    1 => "Inventory Item Updated",
                    _ => "Unexpected Database Error"
                };
            }
            return RedirectToAction("InventoryMain");
        }

        [Authorize]
        public IActionResult LiveInventory()
        {
            ViewData["title"] = "LiveInventory";

            Inventory? inventory = _dbCtx.Inventory.FirstOrDefault();
            if (inventory != null)
            {
                return View(inventory);
            }

            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "/C start vlc rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mp4 :network-caching=1000 :sout=#transcode{vcodec=theo,vb=800,acodec=vorb,ab=128,channels=2,samplerate=44100,scodec=none}:http{mux=ogg,dst=:8080/stream} :sout-all :sout-keep";
            //process.StartInfo = startInfo;
            //process.Start();

            return RedirectToAction("ViewInventory");
        }

        // For remote model validation check
        public IActionResult checkShape(int ShapeId, int InventoryId, int ItemId)
        {
            int currentShape = _dbCtx.InventoryItems.Where(i => i.ItemId == ItemId).First().ShapeId;
            if (currentShape == ShapeId)
            {
                return Json(true);
            }
                                
            InventoryItems? inventoryItems = _dbCtx.InventoryItems
                .Where(i => i.InventoryId == InventoryId && i.ShapeId == ShapeId).FirstOrDefault();
            if (inventoryItems != null)
            {
                return Json("Shape is currently being used by another product");
            }
            return Json(true);
        }

        public IActionResult Dashboard()
        {
            ViewData["title"] = "Dashboard";


            //Daily products sold
            var dailyProductsSold = _dbCtx.ItemsTransaction.Include(i => i.Product)
                .GroupBy(i => new {i.Product.Name, i.LastUpdated!.Value.Day})
                .Select(i => new
            {
                product = i.Key,
                productsSold = i.Sum(q => q.Quantity)
            }).ToExpandoList();
            ViewData["dailyProductsSold"] = dailyProductsSold;

            //Most popular location
            var topProducts = _dbCtx.ItemsTransaction.GroupBy(i => i.Product.Name).Select(i => new
            {
                Name = i.Key,
                Sales = i.Sum(i=> i.Product.Price * i.Quantity)
            }).OrderByDescending(i => i.Sales)
            .ToExpandoList();
            ViewData["topProducts"] = topProducts;

            //No. of times a product hit minQuantity
            var minQuantityHit = _dbCtx.ItemsTransaction.GroupBy(i => i.Product.Name).Select(i => new
            {
                Name = i.Key,
                Count = i.Count(a => a.Quantity == a.MinQuantity -1)
            }).ToExpandoList();
            ViewData["minQuantityHit"] = minQuantityHit;

            //Avg Time taken to topup items
            var items = _dbCtx.ItemsTransaction.Include(i=> i.Product).OrderBy(i=> i.ProductId).ThenBy(i => i.LastUpdated).ToList();
            List<dynamic> topupList = new List<dynamic>();
            
            for (int i = 1; i < items.Count; i++)
            {
                if (items[i].Quantity > items[i - 1].Quantity && items[i].ProductId == items[i-1].ProductId)
                {
                    ItemsTransaction currentItem = items[i];
                    ItemsTransaction? minQuantityItem = checkLastUpdated(i, items, items[i].ProductId);
                    if (minQuantityItem != null)
                    {
                        int timeDiff = currentItem.LastUpdated!.Value.Day - minQuantityItem.LastUpdated!.Value.Day;
                        dynamic item = new
                        {
                            Name = currentItem.Product.Name,
                            Quantity = currentItem.Quantity - minQuantityItem.Quantity,
                            TopupDays = timeDiff
                        }.ToExpando(); 
                        topupList.Add(item);
                    }
                   
                }
            }

            ViewData["topupList"] = topupList.ToExpandoList();


            //Alert on products that that needs hit min quantity
            var alert = _dbCtx.ItemsTransaction.Include(i => i.Product).GroupBy(i => new { i.Product.Name, i.LastUpdated }).Select(i => new
            {
                Name = i.Key.Name.First(),
                LastUpdated = i.Key.LastUpdated
            }).OrderByDescending(i => i.LastUpdated);
            if (alert != null)
            {
                foreach(var a in alert)
                {

                }
            }

            return View();
        }

        public ItemsTransaction? checkLastUpdated(int index, List<ItemsTransaction> items, int productId)
        {
            if (productId != items[index].ProductId)
            {
                return null;
            }
            else if (items[index].MinQuantity == items[index].Quantity && items[index].ProductId == productId)
            {
                return items[index];
            }
            else
            {
                return checkLastUpdated(index - 1, items, productId);
            }
        }
    }
}
