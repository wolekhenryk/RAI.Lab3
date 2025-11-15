using RAI.Lab3.Application.Dto;
using RAI.Lab3.Infrastructure;

namespace RAI.Lab3.Application.Services.Interfaces;

public interface IAvailabilityService
{
    Task<Result<TeacherAvailabilityReadDto>> CreateAvailabilityAsync(
        TeacherAvailabilityCreateDto availabilityCreateDto, CancellationToken ct = default);

    Task<Result<List<TeacherAvailabilityReadDto>>> GetAllAvailabilitiesAsync(CancellationToken ct = default);
    Task<Result> BlockAvailabilityAsync(Guid availabilityId, CancellationToken ct = default);
    Task<Result> UnblockAvailabilityAsync(Guid availabilityId, CancellationToken ct = default);
}