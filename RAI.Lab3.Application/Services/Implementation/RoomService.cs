using Npgsql;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Mapping;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Infrastructure;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;

namespace RAI.Lab3.Application.Services.Implementation;

public class RoomService(IRoomRepository roomRepository, IUnitOfWork unitOfWork) : IRoomService
{
    public async Task<Result<List<RoomReadDto>>> GetAllRoomsAsync(CancellationToken ct = default)
    {
        var rooms = await roomRepository.GetAllAsync(ct);
        var roomDtoList = rooms.MapToReadDto();
        return Result<List<RoomReadDto>>.Success(roomDtoList);
    }

    public async Task<Result<RoomReadDto>> GetRoomByIdAsync(Guid id, CancellationToken ct = default)
    {
        var room = await roomRepository.GetByIdAsync(id, ct);
        if (room is null)
            return Result<RoomReadDto>.Failure(Errors.Db.NotFound());

        var roomDto = room.MapToReadDto();
        return Result<RoomReadDto>.Success(roomDto);
    }

    public async Task<Result<RoomReadDto>> CreateRoomAsync(RoomCreateUpdateDto roomCreateDto,
        CancellationToken ct = default)
    {
        try
        {
            var room = roomCreateDto.MapToRoom();
            var createdRoom = await roomRepository.AddAsync(room, ct);
            await unitOfWork.SaveChangesAsync(ct);

            var roomDto = createdRoom.MapToReadDto();
            return Result<RoomReadDto>.Success(roomDto);
        }
        catch (Exception ex) when (ex.InnerException is PostgresException pgEx)
        {
            return Result<RoomReadDto>.Failure(pgEx.MatchToPostgresException());
        }
    }

    public async Task<Result<RoomReadDto>> UpdateRoomAsync(Guid id, RoomCreateUpdateDto roomUpdateDto,
        CancellationToken ct = default)
    {
        var existingRoom = await roomRepository.GetByIdAsync(id, ct);
        if (existingRoom is null)
            return Result<RoomReadDto>.Failure(Errors.Db.NotFound());

        existingRoom.Name = roomUpdateDto.Name;
        existingRoom.Number = roomUpdateDto.Number;

        try
        {
            var updatedRoom = roomRepository.Update(existingRoom);
            await unitOfWork.SaveChangesAsync(ct);

            var roomDto = updatedRoom.MapToReadDto();
            return Result<RoomReadDto>.Success(roomDto);
        }
        catch (Exception ex) when (ex.InnerException is PostgresException pgEx)
        {
            return Result<RoomReadDto>.Failure(pgEx.MatchToPostgresException());
        }
    }

    public async Task<Result> DeleteRoomAsync(Guid id, CancellationToken ct = default)
    {
        var existingRoom = await roomRepository.GetByIdAsync(id, ct);
        if (existingRoom is null)
            return Result.Failure(Errors.Db.NotFound());

        roomRepository.Delete(existingRoom);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}