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
    
    public partial class Floor
    {
        public Floor()
        {
            this.TableMasters = new HashSet<TableMaster>();
        }
    
        public int Floor_ID { get; set; }
        public string FloorNo { get; set; }
    
        public virtual ICollection<TableMaster> TableMasters { get; set; }
    }
}
