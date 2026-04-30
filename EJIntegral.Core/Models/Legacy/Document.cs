namespace EJIntegral.Core.Models.Legacy;

public class Document
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? DocPath { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? StaffId { get; set; }
}
