//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sadik.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Kindergarten
    {
        public Kindergarten()
        {
            this.Inventories = new HashSet<Inventory>();
            this.Kids = new HashSet<Kid>();
            this.UserKindergartens = new HashSet<UserKindergarten>();
            this.Zones = new HashSet<Zone>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<Kid> Kids { get; set; }
        public virtual ICollection<UserKindergarten> UserKindergartens { get; set; }
        public virtual ICollection<Zone> Zones { get; set; }
    }
}
