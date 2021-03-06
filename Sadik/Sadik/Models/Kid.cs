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
    
    public partial class Kid
    {
        public Kid()
        {
            this.Activities = new HashSet<Activity>();
            this.Presentations = new HashSet<Presentation>();
            this.Skills = new HashSet<Skill>();
            this.ActivityStats = new HashSet<ActivityStat>();
            this.ADRs = new HashSet<ADR>();
            this.CameInClasses = new HashSet<CameInClass>();
            this.EmotionObservations = new HashSet<EmotionObservation>();
            this.ItemUsageDetails = new HashSet<ItemUsageDetail>();
        }
    
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public System.DateTime DateAccepted { get; set; }
        public Nullable<System.DateTime> DateDismissed { get; set; }
        public bool IsDismissed { get; set; }
        public int KindergartenId { get; set; }
    
        public virtual ICollection<Activity> Activities { get; set; }
        public virtual ICollection<Presentation> Presentations { get; set; }
        public virtual ICollection<Skill> Skills { get; set; }
        public virtual ICollection<ActivityStat> ActivityStats { get; set; }
        public virtual ICollection<ADR> ADRs { get; set; }
        public virtual ICollection<CameInClass> CameInClasses { get; set; }
        public virtual ICollection<EmotionObservation> EmotionObservations { get; set; }
        public virtual Kindergarten Kindergarten { get; set; }
        public virtual ICollection<ItemUsageDetail> ItemUsageDetails { get; set; }
    }
}
