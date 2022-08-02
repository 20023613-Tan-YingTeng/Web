using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class Inventory
    {
        public Inventory()
        {
            InventoryItems = new HashSet<InventoryItems>();
            ItemsTransaction = new HashSet<ItemsTransaction>();
            User = new HashSet<Users>();
        }

        public int InventoryId { get; set; }
        public string Location { get; set; } = null!;

        public virtual ICollection<InventoryItems> InventoryItems { get; set; }
        public virtual ICollection<ItemsTransaction> ItemsTransaction { get; set; }

        public virtual ICollection<Users> User { get; set; }
    }
}
