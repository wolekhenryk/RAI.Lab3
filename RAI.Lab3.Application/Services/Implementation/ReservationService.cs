using Microsoft.EntityFrameworkCore;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Mapping;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Infrastructure;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;
using RAI.Lab3.Infrastructure.Security;

namespace RAI.Lab3.Application.Services.Implementation;

public class ReservationService(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IReservationService
{
    public async Task<Result<ReservationReadDto>> CreateReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        var reservation = await reservationRepository.Query()
            .Include(r => r.Student)
            .Include(r => r.TeacherAvailability)
            .FirstOrDefaultAsync(r => r.Id == reservationId, ct);

        if (reservation is null)
            return Result<ReservationReadDto>.Failure(Errors.Db.NotFound("Slot not found."));

        if (reservation.StudentId is not null)
            return Result<ReservationReadDto>.Failure(new Error("reservation.already_taken", "This slot is already taken."));

        if (reservation.TeacherAvailability.IsBlocked)
            return Result<ReservationReadDto>.Failure(new Error("reservation.blocked", "This availability is blocked."));

        if (reservation.Period.LowerBound <= DateTime.UtcNow)
            return Result<ReservationReadDto>.Failure(new Error("reservation.past", "Cannot sign up for past slots."));

        reservation.StudentId = currentUserService.UserId;
        reservationRepository.Update(reservation);
        await unitOfWork.SaveChangesAsync(ct);

        var reservationDto = reservation.MapToReadDto();
        return Result<ReservationReadDto>.Success(reservationDto);
    }

    public async Task<Result<ReservationReadDto>> CreateReservationIllegallyAsync(Guid reservationId, Guid studentId, CancellationToken ct = default)
    {
        var reservation = await reservationRepository.Query()
            .Include(r => r.Student)
            .Include(r => r.TeacherAvailability)
            .FirstOrDefaultAsync(r => r.Id == reservationId, ct);

        if (reservation is null)
            return Result<ReservationReadDto>.Failure(Errors.Db.NotFound("Slot not found."));

        if (reservation.StudentId is not null)
            return Result<ReservationReadDto>.Failure(new Error("reservation.already_taken", "This slot is already taken."));

        if (reservation.TeacherAvailability.IsBlocked)
            return Result<ReservationReadDto>.Failure(new Error("reservation.blocked", "This availability is blocked."));

        if (reservation.Period.LowerBound <= DateTime.UtcNow)
            return Result<ReservationReadDto>.Failure(new Error("reservation.past", "Cannot sign up for past slots."));

        reservation.StudentId = studentId;
        reservationRepository.Update(reservation);
        await unitOfWork.SaveChangesAsync(ct);

        var reservationDto = reservation.MapToReadDto();
        return Result<ReservationReadDto>.Success(reservationDto);    }

    public async Task<Result> DeleteReservationAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.Failure(Errors.Db.NotFound());

        reservationRepository.Delete(reservation);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}