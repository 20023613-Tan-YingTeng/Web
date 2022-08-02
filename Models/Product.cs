using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class Product
    {
        public Product()
        {
            InventoryItems = new HashSet<InventoryItems>();
            ItemsTransaction = new HashSet<ItemsTransaction>();
        }

        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public float Price { get; set; }

        public virtual ICollection<InventoryItems> InventoryItems { get; set; }
        public virtual ICollection<ItemsTransaction> ItemsTransaction { get; set; }
    }
}
