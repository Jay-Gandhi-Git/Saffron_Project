﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

public partial class SaffronModel : DbContext
{
    public SaffronModel()
        : base("name=SaffronModel")
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }

    public virtual DbSet<Admin> Admins { get; set; }
    public virtual DbSet<CustomerDetail> CustomerDetails { get; set; }
    public virtual DbSet<Floor> Floors { get; set; }
    public virtual DbSet<ItemCategory> ItemCategories { get; set; }
    public virtual DbSet<ItemMaster> ItemMasters { get; set; }
    public virtual DbSet<ItemUsedStock> ItemUsedStocks { get; set; }
    public virtual DbSet<OrderDetailMaster> OrderDetailMasters { get; set; }
    public virtual DbSet<OrderMaster> OrderMasters { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Purchase> Purchases { get; set; }
    public virtual DbSet<PurchaseDetail> PurchaseDetails { get; set; }
    public virtual DbSet<TableType> TableTypes { get; set; }
    public virtual DbSet<Vendor> Vendors { get; set; }
    public virtual DbSet<WaiterAttendence> WaiterAttendences { get; set; }
    public virtual DbSet<WaiterMaster> WaiterMasters { get; set; }
    public virtual DbSet<TableMaster> TableMasters { get; set; }
    public virtual DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public virtual DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
}
