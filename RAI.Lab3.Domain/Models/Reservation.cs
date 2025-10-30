using NpgsqlTypes;

namespace RAI.Lab3.Domain.Models;

public class Reservation : BaseDbEntity
{
    public Guid TeacherAvailabilityId { get; set; }
    public virtual TeacherAvailability TeacherAvailability { get; set; } = null!;
    
    public NpgsqlRange<DateTime> Period { get; set; }

    public Guid? StudentId { get; set; }
    public virtual User? Student { get; set; }
}