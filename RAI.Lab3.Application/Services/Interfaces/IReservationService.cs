using RAI.Lab3.Application.Dto;
using RAI.Lab3.Infrastructure;

namespace RAI.Lab3.Application.Services.Interfaces;

public interface IReservationService
{
    Task<Result<ReservationReadDto>> CreateReservationAsync(Guid reservationId, CancellationToken ct = default);
    Task<Result<ReservationReadDto>> CreateReservationIllegallyAsync(Guid reservationId, Guid studentId, CancellationToken ct = default);
    Task<Result> DeleteReservationAsync(Guid id, CancellationToken ct = default);
}