//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Duncan.PEMS.DataAccess.PEMS
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmailTemplateType
    {
        public EmailTemplateType()
        {
            this.DiscountSchemeEmailTemplates = new HashSet<DiscountSchemeEmailTemplate>();
        }
    
        public int EmailTemplateTypeId { get; set; }
        public string EmailTemplateTypeDesc { get; set; }
    
        public virtual ICollection<DiscountSchemeEmailTemplate> DiscountSchemeEmailTemplates { get; set; }
    }
}
