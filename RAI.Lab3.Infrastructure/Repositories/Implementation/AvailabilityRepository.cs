using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;

namespace RAI.Lab3.Infrastructure.Repositories.Implementation;

public class AvailabilityRepository(AppDbContext dbContext)
    : BaseRepository<TeacherAvailability>(dbContext), IAvailabilityRepository;