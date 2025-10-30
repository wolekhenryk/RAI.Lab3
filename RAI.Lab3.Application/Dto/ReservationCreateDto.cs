namespace RAI.Lab3.Application.Dto;

public class ReservationCreateDto
{
    public Guid TeacherAvailabilityId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}