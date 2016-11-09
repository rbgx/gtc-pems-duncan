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
    
    public partial class FDFileType
    {
        public FDFileType()
        {
            this.FDFileTypeMeterGroups = new HashSet<FDFileTypeMeterGroup>();
            this.FDHousings = new HashSet<FDHousing>();
            this.FDHousingAudits = new HashSet<FDHousingAudit>();
            this.FileTypeMaps = new HashSet<FileTypeMap>();
            this.FDFiles = new HashSet<FDFile>();
        }
    
        public int FileType { get; set; }
        public string FileDesc { get; set; }
        public string FileExtension { get; set; }
    
        public virtual ICollection<FDFileTypeMeterGroup> FDFileTypeMeterGroups { get; set; }
        public virtual ICollection<FDHousing> FDHousings { get; set; }
        public virtual ICollection<FDHousingAudit> FDHousingAudits { get; set; }
        public virtual ICollection<FileTypeMap> FileTypeMaps { get; set; }
        public virtual ICollection<FDFile> FDFiles { get; set; }
    }
}
