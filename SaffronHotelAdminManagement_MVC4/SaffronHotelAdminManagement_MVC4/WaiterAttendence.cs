//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SaffronHotelAdminManagement_MVC4
{
    using System;
    using System.Collections.Generic;
    
    public partial class WaiterAttendence
    {
        public int WaiterAttendence_Id { get; set; }
        public Nullable<int> WaiterMaster_Id { get; set; }
        public Nullable<System.DateTime> DateTime { get; set; }
    
        public virtual WaiterMaster WaiterMaster { get; set; }
    }
}
