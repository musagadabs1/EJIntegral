namespace EJIntegral.Core.Models.Legacy;

public class Designation
{
    public int Id { get; set; }
    public int? DeparmentId { get; set; }
    public string? DesignationName { get; set; }
    public bool? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
