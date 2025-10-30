namespace RAI.Lab3.Application.Dto;

public class TeacherAvailabilityCreateDto
{
    public Guid RoomId { get; set; }
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public TimeOnly StartTime { get; set; } = new(DateTime.UtcNow.Hour, 0);
    public TimeOnly EndTime { get; set; } = new(DateTime.UtcNow.Hour + 1, 0);
    public int SlotDurationMinutes { get; set; } = 30;
}