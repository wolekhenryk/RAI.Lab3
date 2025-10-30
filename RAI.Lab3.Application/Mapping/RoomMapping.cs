using RAI.Lab3.Application.Dto;
using RAI.Lab3.Domain.Models;

namespace RAI.Lab3.Application.Mapping;

public static class RoomMapping
{
    public static RoomReadDto MapToReadDto(this Room room)
    {
        return new RoomReadDto
        {
            Id = room.Id,
            Name = room.Name,
            Number = room.Number
        };
    }
    
    public static List<RoomReadDto> MapToReadDto(this List<Room> rooms)
    {
        return rooms.Select(r => r.MapToReadDto()).ToList();
    }

    public static Room MapToRoom(this RoomCreateUpdateDto roomCreateDto)
    {
        return new Room
        {
            Name = roomCreateDto.Name,
            Number = roomCreateDto.Number
        };
    }
}