﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

public partial class Admin
{
    public int Admin_ID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Nullable<bool> IsActive { get; set; }
}

public partial class CustomerDetail
{
    public CustomerDetail()
    {
        this.OrderMasters = new HashSet<OrderMaster>();
    }

    public int Customer_ID { get; set; }
    public string CustomerName { get; set; }
    public string Address { get; set; }
    public string MobileNo { get; set; }
    public string Email { get; set; }

    public virtual ICollection<OrderMaster> OrderMasters { get; set; }
}

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

public partial class ItemCategory
{
    public ItemCategory()
    {
        this.ItemMasters = new HashSet<ItemMaster>();
    }

    public int ItemCategory_ID { get; set; }
    public string ItemCategoryName { get; set; }
    public Nullable<bool> IsMenuItem { get; set; }
    public Nullable<bool> IsAssets { get; set; }
    public Nullable<bool> IsService { get; set; }
    public Nullable<bool> IsRawMetirial { get; set; }

    public virtual ICollection<ItemMaster> ItemMasters { get; set; }
}

public partial class ItemMaster
{
    public ItemMaster()
    {
        this.ItemUsedStocks = new HashSet<ItemUsedStock>();
        this.OrderDetailMasters = new HashSet<OrderDetailMaster>();
        this.PurchaseDetails = new HashSet<PurchaseDetail>();
        this.PurchaseReturnDetails = new HashSet<PurchaseReturnDetail>();
    }

    public int ItemMaster_ID { get; set; }
    public Nullable<int> ItemCategory_ID { get; set; }
    public string ItemName { get; set; }
    public Nullable<bool> IsVeg { get; set; }
    public Nullable<double> Rate { get; set; }
    public Nullable<double> CGST { get; set; }
    public Nullable<double> SGST { get; set; }
    public Nullable<int> ReOrderLevel { get; set; }
    public Nullable<int> MinimumQuantatity { get; set; }

    public virtual ItemCategory ItemCategory { get; set; }
    public virtual ICollection<ItemUsedStock> ItemUsedStocks { get; set; }
    public virtual ICollection<OrderDetailMaster> OrderDetailMasters { get; set; }
    public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
}

public partial class ItemUsedStock
{
    public int Stock_ID { get; set; }
    public Nullable<int> ItemMaster_ID { get; set; }
    public Nullable<System.DateTime> Date { get; set; }
    public Nullable<int> Quantity { get; set; }

    public virtual ItemMaster ItemMaster { get; set; }
}

public partial class OrderDetailMaster
{
    public int OrderDetail_ID { get; set; }
    public Nullable<int> OrderMaster_ID { get; set; }
    public Nullable<int> ItemMaster_ID { get; set; }
    public Nullable<int> Quantity { get; set; }
    public Nullable<double> Rate { get; set; }
    public Nullable<double> Amount { get; set; }
    public Nullable<bool> IsKOT { get; set; }
    public Nullable<bool> IsKOTProcess { get; set; }
    public Nullable<bool> IsKOTComplete { get; set; }

    public virtual ItemMaster ItemMaster { get; set; }
    public virtual OrderMaster OrderMaster { get; set; }
}

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

public partial class Payment
{
    public int Paymnet_Id { get; set; }
    public Nullable<int> OrderMaster_Id { get; set; }
    public string PaymentMode { get; set; }
    public Nullable<double> Amount { get; set; }
    public Nullable<System.DateTime> PayementDate { get; set; }
    public string Remarks { get; set; }

    public virtual OrderMaster OrderMaster { get; set; }
}

public partial class Purchase
{
    public Purchase()
    {
        this.PurchaseDetails = new HashSet<PurchaseDetail>();
        this.PurchaseReturns = new HashSet<PurchaseReturn>();
    }

    public int Purchase_ID { get; set; }
    public Nullable<int> Vendor_ID { get; set; }
    public Nullable<System.DateTime> Date { get; set; }
    public string BillNo { get; set; }

    public virtual Vendor Vendor { get; set; }
    public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; }
}

public partial class PurchaseDetail
{
    public int PurchaseDetail_ID { get; set; }
    public Nullable<int> Purchase_ID { get; set; }
    public Nullable<int> ItemMaster_ID { get; set; }
    public Nullable<int> Quantity { get; set; }
    public Nullable<double> Rate { get; set; }
    public Nullable<double> Amount { get; set; }

    public virtual ItemMaster ItemMaster { get; set; }
    public virtual Purchase Purchase { get; set; }
}

public partial class PurchaseReturn
{
    public PurchaseReturn()
    {
        this.PurchaseReturnDetails = new HashSet<PurchaseReturnDetail>();
    }

    public int PurchaseReturn_ID { get; set; }
    public Nullable<int> Purchase_ID { get; set; }
    public Nullable<System.DateTime> Date { get; set; }

    public virtual Purchase Purchase { get; set; }
    public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
}

public partial class PurchaseReturnDetail
{
    public int PurchaseReturnDetail_ID { get; set; }
    public Nullable<int> PurchaseReturn_ID { get; set; }
    public Nullable<int> ItemMaster_ID { get; set; }
    public Nullable<int> Quatntity { get; set; }
    public string Remarks { get; set; }

    public virtual ItemMaster ItemMaster { get; set; }
    public virtual PurchaseReturn PurchaseReturn { get; set; }
}

public partial class TableMaster
{
    public TableMaster()
    {
        this.OrderMasters = new HashSet<OrderMaster>();
    }

    public int Table_ID { get; set; }
    public Nullable<int> Floor_ID { get; set; }
    public Nullable<int> TableType_ID { get; set; }
    public Nullable<int> TableCapacity { get; set; }
    public string TableNo { get; set; }

    public virtual Floor Floor { get; set; }
    public virtual ICollection<OrderMaster> OrderMasters { get; set; }
    public virtual TableType TableType { get; set; }
}

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

public partial class Vendor
{
    public Vendor()
    {
        this.Purchases = new HashSet<Purchase>();
    }

    public int Vendor_ID { get; set; }
    public string VendorName { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public string MobileNo { get; set; }
    public string GstNo { get; set; }
    public string Email { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; }
}

public partial class WaiterAttendence
{
    public int WaiterAttendence_Id { get; set; }
    public Nullable<int> WaiterMaster_Id { get; set; }
    public Nullable<System.DateTime> DateTime { get; set; }

    public virtual WaiterMaster WaiterMaster { get; set; }
}

public partial class WaiterMaster
{
    public WaiterMaster()
    {
        this.OrderMasters = new HashSet<OrderMaster>();
        this.WaiterAttendences = new HashSet<WaiterAttendence>();
    }

    public int WaiterMaster_Id { get; set; }
    public string WaiterName { get; set; }
    public string ContctNo { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string AdharCardNo { get; set; }

    public virtual ICollection<OrderMaster> OrderMasters { get; set; }
    public virtual ICollection<WaiterAttendence> WaiterAttendences { get; set; }
}
