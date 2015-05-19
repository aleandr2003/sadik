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
    
    public partial class Inventory
    {
        public Inventory()
        {
            this.Activities = new HashSet<Activity>();
            this.Presentations = new HashSet<Presentation>();
            this.ActivityStats = new HashSet<ActivityStat>();
            this.ItemUsageDetails = new HashSet<ItemUsageDetail>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public int ParentZoneId { get; set; }
        public int KindergartenId { get; set; }
    
        public virtual ICollection<Activity> Activities { get; set; }
        public virtual ICollection<Presentation> Presentations { get; set; }
        public virtual ICollection<ActivityStat> ActivityStats { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual Kindergarten Kindergarten { get; set; }
        public virtual ICollection<ItemUsageDetail> ItemUsageDetails { get; set; }
    }
}
