using System;
using System.Collections.Generic;

namespace xShapes.Models
{
    public partial class Users
    {
        public Users()
        {
            Inventory = new HashSet<Inventory>();
        }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!;
        public byte[] Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ContactNo { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<Inventory> Inventory { get; set; }
    }
}
