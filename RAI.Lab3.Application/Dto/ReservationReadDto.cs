namespace RAI.Lab3.Application.Dto;

public class ReservationReadDto
{
    public Guid Id { get; set; }
    public Guid TeacherAvailabilityId { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    
    public string? StudentFullName { get; set; }
    public bool IsReserved => StudentFullName != null;
}