using NpgsqlTypes;

namespace RAI.Lab3.Domain.Models;

public class TeacherAvailability : BaseDbEntity
{
    public Guid TeacherId { get; set; }
    public virtual User Teacher { get; set; } = null!;

    public Guid RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;
    
    public NpgsqlRange<DateTime>[] Periods { get; set; } = [];

    public int SlotDurationMinutes { get; set; }
    public bool IsBlocked { get; set; } = false;

    public virtual ICollection<Reservation> Reservations { get; set; } = [];
}