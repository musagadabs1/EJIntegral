using System;

namespace EJIntegral.ViewModels
{
    public class StaffDetailsViewModel
    {
        public string StaffId { get; set; }
        public string Fullname { get; set; }
        public Nullable<DateTime> DateOfBirth { get; set; }
        public string ContactNumber { get; set; }
        public string DepartmentName { get; set; }
        public string EntryDesignation { get; set; }
        public string EntryGradeLevel { get; set; }
        public Nullable<int> EntryStep { get; set; }
        public string CurrentGradeLevel { get; set; }
        public Nullable<int> CurrentStep { get; set; }
        public string EntryQualification { get; set; }
        public string AdditionalQualification { get; set; }
        public Nullable<DateTime> DateOfFirstAppt { get; set; }
    }
}