namespace RAI.Lab3.Domain;

public abstract class BaseDbEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}