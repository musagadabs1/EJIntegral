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
    
    public partial class Staff_Details
    {
        public int Id { get; set; }
        public string StaffId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string ResidenceAddress { get; set; }
        public string MaritalStatus { get; set; }
        public string ContactPhone { get; set; }
        public string EmailAddress { get; set; }
        public string StateOfOrigin { get; set; }
        public Nullable<int> LGA { get; set; }
        public string NFullName { get; set; }
        public string NAddress { get; set; }
        public string NRelationship { get; set; }
        public string NContactNumber { get; set; }
        public Nullable<bool> Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string DevArea { get; set; }
    }
}
