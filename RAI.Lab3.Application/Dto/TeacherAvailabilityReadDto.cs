namespace RAI.Lab3.Application.Dto;

public class TeacherAvailabilityReadDto
{
    public Guid Id { get; set; }
    public string TeacherFullName { get; set; }
    public string RoomName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int AmountOfSlots { get; set; }
    public int AmountOfReservations { get; set; }
}