//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duncan.PEMS.DataAccess.RBAC
{
    using System;
    using System.Collections.Generic;
    
    public partial class CustomerGrid
    {
        public CustomerGrid()
        {
            this.User_CustomerGrids = new HashSet<User_CustomerGrids>();
        }
    
        public int CustomerGridsId { get; set; }
        public int CustomerId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }
        public string OriginalTitle { get; set; }
        public int OriginalPosition { get; set; }
        public bool IsHidden { get; set; }
    
        public virtual CustomerProfile CustomerProfile { get; set; }
        public virtual ICollection<User_CustomerGrids> User_CustomerGrids { get; set; }
    }
}