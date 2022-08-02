using Microsoft.AspNetCore.Mvc;

namespace xShapes.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _dbCtx;

        public ProductController(AppDbContext dbContext)
        {
            _dbCtx = dbContext;
        }
        public IActionResult ProductMain()
        {
            ViewData["title"] = "Product";
            ViewData["name"] = User.FindFirst(ClaimTypes.Name)!.Value;
            ViewData["row"] = User.FindFirst(ClaimTypes.Role)!.Value;

            List<Product> model = null!;
            Product product = _dbCtx.Product
                                .First();
            model = _dbCtx.Product
               .ToList();

            return View(model);
        }

        [Authorize]
        public IActionResult CreateProduct()
        {
            ViewData["title"] = "CreateProduct";

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {

                _dbCtx.Product.Add(product);

                if (_dbCtx.SaveChanges() == 1)
                    TempData["Msg"] = "New product added!";
                else
                    TempData["Msg"] = "Failed to update database!";
            }
            else
            {
                TempData["Msg"] = "Invalid information entered";
            }
            return RedirectToAction("ProductMain");
        }

        public IActionResult Update(int id)
        {
            ViewData["title"] = "Update";
            Product product = _dbCtx.Product
                .Where(i => i.ProductId == id)
                .First(); 
            return View(product);
        }

        [HttpPost]
        public IActionResult Update(Product product)
        {
            if (!ModelState.IsValid)
            {
                TempData["Msg"] = "Invalid Information Entered";
                return RedirectToAction("ProductMain");
            }

            Product? updatedProduct = _dbCtx.Product
                .Where(i => i.ProductId == product.ProductId)
                .First();

            if (updatedProduct == null)
            {
                TempData["Msg"] = "Product Not Found";
            }
            else
            {

                // Replace current in database with new value in the list
                updatedProduct.ProductId = product.ProductId;
                updatedProduct.Name = product.Name;
                updatedProduct.Price = product.Price;


                TempData["Msg"] = _dbCtx.SaveChanges() switch
                {
                    0 => "No Changes Made",
                    1 => "Inventory Item Updated",
                    _ => "Unexpected Database Error"
                };
            }
            return RedirectToAction("ProductMain");
        }

        public IActionResult Delete(int id)
        {
            DbSet<Product> dbs = _dbCtx.Product;
            Product? product = dbs
                .Where(i => i.ProductId == id)
                .FirstOrDefault();

            if (product == null)
            {
                TempData["Msg"] = "Product Not Found";
                return RedirectToAction("ProductMain");
            }

            List<InventoryItems> itemList = _dbCtx.InventoryItems
                .Where(i => i.ProductId == product.ProductId).ToList();
            if (itemList.Count != 0)
            {
                TempData["Msg"] = "Product is being used in Inventory";
                return RedirectToAction("ProductMain");
            }
            dbs.Remove(product);
            TempData["Msg"] = _dbCtx.SaveChanges() switch
            {
                1 => "Product Deleted",
                _ => "Unexpected Database Error"
            };

            return RedirectToAction("ProductMain");
        }


    }
}
