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
    
    public partial class ActivityStat
    {
        public int Id { get; set; }
        public int KidId { get; set; }
        public int ItemId { get; set; }
        public int Brief { get; set; }
        public int Short { get; set; }
        public int Long { get; set; }
        public int Polarization { get; set; }
        public int ChoseHimSelf { get; set; }
    
        public virtual Inventory Inventory { get; set; }
        public virtual Kid Kid { get; set; }
    }
}
