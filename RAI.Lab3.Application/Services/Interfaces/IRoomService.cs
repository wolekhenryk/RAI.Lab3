using RAI.Lab3.Application.Dto;
using RAI.Lab3.Infrastructure;

namespace RAI.Lab3.Application.Services.Interfaces;

public interface IRoomService
{
    Task<Result<List<RoomReadDto>>> GetAllRoomsAsync(CancellationToken ct = default);
    Task<Result<RoomReadDto>> GetRoomByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<RoomReadDto>> CreateRoomAsync(RoomCreateUpdateDto roomCreateDto, CancellationToken ct = default);

    Task<Result<RoomReadDto>> UpdateRoomAsync(Guid id, RoomCreateUpdateDto roomUpdateDto,
        CancellationToken ct = default);

    Task<Result> DeleteRoomAsync(Guid id, CancellationToken ct = default);
    
    Task<byte[]> ExportToTxtAsync(Guid roomId, CancellationToken ct = default);
    Task<byte[]> ExportToCsvAsync(Guid roomId, CancellationToken ct = default);
    Task<byte[]> ExportToPdfAsync(Guid roomId, CancellationToken ct = default);
}