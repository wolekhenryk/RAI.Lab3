using NpgsqlTypes;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Helpers;
using RAI.Lab3.Domain.Models;

namespace RAI.Lab3.Application.Mapping;

public static class AvailabilityMapping
{
    public static TeacherAvailability MapFromDto(this TeacherAvailabilityCreateDto createDto, Guid teacherId)
    {
        var amountOfDays = createDto.EndDate.DayNumber - createDto.StartDate.DayNumber + 1;
        var amountOfMinutes = (createDto.EndTime - createDto.StartTime).TotalMinutes;

        var slotsInOneDay = amountOfMinutes / createDto.SlotDurationMinutes;

        List<NpgsqlRange<DateTime>> slots = [];

        for (var i = 0; i < amountOfDays; i++)
        {
            var dateOnly = createDto.StartDate.AddDays(i);
            for (var j = 0; j < slotsInOneDay; j++)
            {
                var timeOnly = createDto.StartTime.AddMinutes(j * createDto.SlotDurationMinutes);

                var dateTimeStart = dateOnly.ToDateTime(timeOnly);
                var dateTimeEnd = dateTimeStart.AddMinutes(createDto.SlotDurationMinutes);

                var dateTimeStartUtc = TimeZoneHelper.ToUtcFromZone(dateTimeStart);
                var dateTimeEndUtc = TimeZoneHelper.ToUtcFromZone(dateTimeEnd);

                var range = new NpgsqlRange<DateTime>(dateTimeStartUtc, true, dateTimeEndUtc, false);
                slots.Add(range);
            }
        }

        return new TeacherAvailability
        {
            RoomId = createDto.RoomId,
            TeacherId = teacherId,
            Periods = slots.ToArray(),
            SlotDurationMinutes = createDto.SlotDurationMinutes
        };
    }

    public static TeacherAvailabilityReadDto MapToReadDto(this TeacherAvailability availability)
    {
        var startDateTimeUtc = availability.Periods.Min(p => p.LowerBound);
        var endDateTimeUtc = availability.Periods.Max(p => p.UpperBound);
        
        var startDateTime = TimeZoneHelper.ToZoneFromUtc(startDateTimeUtc);
        var endDateTime = TimeZoneHelper.ToZoneFromUtc(endDateTimeUtc);
        
        return new TeacherAvailabilityReadDto
        {
            Id = availability.Id,
            TeacherFullName = $"{availability.Teacher.FirstName} {availability.Teacher.LastName}",
            RoomName = availability.Room.Name,
            StartDate = DateOnly.FromDateTime(startDateTime),
            EndDate = DateOnly.FromDateTime(endDateTime),
            StartTime = TimeOnly.FromDateTime(startDateTime),
            EndTime = TimeOnly.FromDateTime(endDateTime),
            AmountOfSlots = availability.Periods.Length,
            AmountOfReservations = availability.Reservations.Count
        };
    }

    public static List<TeacherAvailabilityReadDto> MapToReadDto(this List<TeacherAvailability> availability)
    {
        return availability.Select(a => a.MapToReadDto()).ToList();
    }
}