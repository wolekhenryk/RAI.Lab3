using Microsoft.AspNetCore.Identity;

namespace RAI.Lab3.Domain.Models;

public class User : IdentityUser<Guid>
{
    public virtual ICollection<TeacherAvailability> Availabilities { get; set; } = [];
    public virtual ICollection<Reservation> Reservations { get; set; } = [];
    
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}