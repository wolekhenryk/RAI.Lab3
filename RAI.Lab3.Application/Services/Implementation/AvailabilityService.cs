using Npgsql;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Mapping;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;
using RAI.Lab3.Infrastructure.Security;

namespace RAI.Lab3.Application.Services.Implementation;

public class AvailabilityService(
    IAvailabilityRepository repository,
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IAvailabilityService
{
    public async Task<Result<TeacherAvailabilityReadDto>> CreateAvailabilityAsync(
        TeacherAvailabilityCreateDto availabilityCreateDto, CancellationToken ct = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        
        try
        {
            var availability = availabilityCreateDto.MapFromDto(currentUserService.UserId);

            var reservations = availability
                .Periods
                .Select(ap => new Reservation
                {
                    TeacherAvailabilityId = availability.Id,
                    Period = ap
                })
                .ToList();
            
            var createdAvailability = await repository.AddAsync(availability, ct);
            await reservationRepository.AddAsync(reservations, ct);
            
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            
            var availabilityDto = createdAvailability.MapToReadDto();
            return Result<TeacherAvailabilityReadDto>.Success(availabilityDto);
        }
        catch (Exception ex) when (ex.InnerException is PostgresException pgEx)
        {
            await transaction.RollbackAsync(ct);
            return Result<TeacherAvailabilityReadDto>.Failure(pgEx.MatchToPostgresException());
        }
    }

    public async Task<Result<List<TeacherAvailabilityReadDto>>> GetAllAvailabilitiesAsync(
        CancellationToken ct = default)
    {
        var availabilities = await repository.GetAllAsync(ct);
        var availabilityDtoList = availabilities.MapToReadDto();
        return Result<List<TeacherAvailabilityReadDto>>.Success(availabilityDtoList);
    }
}