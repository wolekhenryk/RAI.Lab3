namespace RAI.Lab3.Domain.Models;

public class Room : BaseDbEntity
{
    public string Name { get; set; } = null!;
    public int Number { get; set; }

    public virtual ICollection<TeacherAvailability> TeacherAvailabilities { get; set; } = [];
}