namespace AgileFlow.Core.Entities;

public class Issue
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
}
