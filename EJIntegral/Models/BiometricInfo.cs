//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EJIntegral.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BiometricInfo
    {
        public int Id { get; set; }
        public string StaffId { get; set; }
        public string template { get; set; }
        public Nullable<int> imageWidth { get; set; }
        public Nullable<int> imageHeight { get; set; }
        public Nullable<int> imageDPI { get; set; }
        public Nullable<int> imageQuality { get; set; }
        public string fingerPosition { get; set; }
        public string serialNumber { get; set; }
        public string model { get; set; }
        public string manufacturer { get; set; }
        public Nullable<int> creator { get; set; }
        public Nullable<System.DateTime> date_created { get; set; }
    }
}
