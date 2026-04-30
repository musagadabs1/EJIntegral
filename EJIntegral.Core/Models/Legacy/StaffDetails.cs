namespace EJIntegral.Core.Models.Legacy;

public class StaffDetails
{
    public int Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? MaritalStatus { get; set; }
    public string? ContactPhone { get; set; }
    public string? EmailAddress { get; set; }
    public string? StateOfOrigin { get; set; }
    public int? LGA { get; set; }
    public string? NFullName { get; set; }
    public string? NAddress { get; set; }
    public string? NRelationship { get; set; }
    public string? NContactNumber { get; set; }
    public bool? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? DevArea { get; set; }
}
