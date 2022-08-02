using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class Shape
    {
        public Shape()
        {
            InventoryItems = new HashSet<InventoryItems>();
        }

        public int ShapeId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<InventoryItems> InventoryItems { get; set; }
    }
}
