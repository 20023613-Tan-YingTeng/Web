using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class ItemsTransaction
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual Inventory Inventory { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
