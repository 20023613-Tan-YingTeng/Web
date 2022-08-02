using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class InventoryItems
    {
        public int ItemId { get; set; }
        public int ShapeId { get; set; }
        public int ProductId { get; set; }
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual Inventory Inventory { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual Shape Shape { get; set; } = null!;
    }
}
