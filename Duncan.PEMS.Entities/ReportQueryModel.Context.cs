﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duncan.PEMS.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ReportQueryEntities : DbContext
    {
        public ReportQueryEntities()
            : base("name=ReportQueryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<CustomerSetting> CustomerSettings { get; set; }
        public DbSet<UserCustomerAccess> UserCustomerAccesses { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}