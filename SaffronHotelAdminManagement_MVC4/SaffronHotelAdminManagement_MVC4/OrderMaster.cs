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
    
    public partial class OrderMaster
    {
        public OrderMaster()
        {
            this.OrderDetailMasters = new HashSet<OrderDetailMaster>();
            this.Payments = new HashSet<Payment>();
        }
    
        public int OrderMaster_ID { get; set; }
        public string OrderType { get; set; }
        public Nullable<int> Table_ID { get; set; }
        public Nullable<int> WaiterMaster_Id { get; set; }
        public Nullable<int> Customer_ID { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<bool> IsComplete { get; set; }
        public Nullable<bool> IsCancle { get; set; }
        public Nullable<bool> IsProcess { get; set; }
        public Nullable<int> BillNo { get; set; }
    
        public virtual CustomerDetail CustomerDetail { get; set; }
        public virtual ICollection<OrderDetailMaster> OrderDetailMasters { get; set; }
        public virtual WaiterMaster WaiterMaster { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual TableMaster TableMaster { get; set; }
    }
}