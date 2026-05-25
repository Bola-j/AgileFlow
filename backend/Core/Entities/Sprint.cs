namespace AgileFlow.Core.Entities;

public class Sprint
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid ProjectId { get; set; }
}
