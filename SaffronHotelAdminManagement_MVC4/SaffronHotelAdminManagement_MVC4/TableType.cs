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
    
    public partial class TableType
    {
        public TableType()
        {
            this.TableMasters = new HashSet<TableMaster>();
        }
    
        public int TableType_ID { get; set; }
        public string TableName { get; set; }
    
        public virtual ICollection<TableMaster> TableMasters { get; set; }
    }
}