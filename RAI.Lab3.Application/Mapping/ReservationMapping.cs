using NpgsqlTypes;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Helpers;
using RAI.Lab3.Domain.Models;

namespace RAI.Lab3.Application.Mapping;

public static class ReservationMapping
{
    public static Reservation MapToReservation(this ReservationCreateDto createDto)
    {
        return new Reservation
        {
            TeacherAvailabilityId = createDto.TeacherAvailabilityId,
            Period = new NpgsqlRange<DateTime>(createDto.StartUtc, true, createDto.EndUtc, false)
        };
    }
    
    public static ReservationReadDto MapToReadDto(this Reservation reservation)
    {
        return new ReservationReadDto
        {
            Id = reservation.Id,
            TeacherAvailabilityId = reservation.TeacherAvailabilityId,
            StartLocal = TimeZoneHelper.ToZoneFromUtc(reservation.Period.LowerBound),
            EndLocal = TimeZoneHelper.ToZoneFromUtc(reservation.Period.UpperBound),
            StudentFullName = reservation.Student is not null
                ? $"{reservation.Student.FirstName} {reservation.Student.LastName}"
                : null
        };
    }
}