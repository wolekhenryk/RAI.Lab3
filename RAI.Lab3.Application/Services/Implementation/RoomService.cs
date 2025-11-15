using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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

    public async Task<byte[]> ExportToTxtAsync(Guid roomId, CancellationToken ct = default)
    {
        var reservations = await roomRepository.Query()
            .Where(r => r.Id == roomId)
            .Include(r => r.TeacherAvailabilities)
            .ThenInclude(t => t.Reservations)
            .AsSplitQuery()
            .SelectMany(t => t.TeacherAvailabilities)
            .SelectMany(t => t.Reservations)
            .Select(r => r.MapToReadDto())
            .ToListAsync(cancellationToken: ct);
        
        var json = JsonSerializer.Serialize(reservations, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        
        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<byte[]> ExportToCsvAsync(Guid roomId, CancellationToken ct = default)
    {
        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        var reservations = await roomRepository.Query()
            .Where(r => r.Id == roomId)
            .Include(r => r.TeacherAvailabilities)
            .ThenInclude(t => t.Reservations)
            .SelectMany(t => t.TeacherAvailabilities)
            .SelectMany(t => t.Reservations)
            .AsSplitQuery()
            .Select(r => r.MapToReadDto())
            .ToListAsync(cancellationToken: ct);
        
        await csv.WriteRecordsAsync(reservations, ct);
        await writer.FlushAsync(ct);
        
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(Guid roomId, CancellationToken ct = default)
    {
        var reservations = await roomRepository.Query()
            .Where(r => r.Id == roomId)
            .Include(r => r.TeacherAvailabilities)
            .ThenInclude(t => t.Reservations)
            .SelectMany(t => t.TeacherAvailabilities)
            .SelectMany(t => t.Reservations)
            .AsSplitQuery()
            .Select(r => r.MapToReadDto())
            .ToListAsync(cancellationToken: ct);

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10);
                page.DefaultTextStyle(TextStyle.Default.FontSize(8));
                
                page.Header()
                    .Text("List of reservations")
                    .SemiBold()
                    .FontSize(10)
                    .AlignCenter();
                
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.5f);
                        c.RelativeColumn(1.35f);
                        c.RelativeColumn(1.35f);
                        c.RelativeColumn(1.35f);
                        c.RelativeColumn(1.35f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Reservation Id");
                        header.Cell().Element(HeaderCell).Text("Start");
                        header.Cell().Element(HeaderCell).Text("End");
                        header.Cell().Element(HeaderCell).Text("Student Full Name");
                        header.Cell().Element(HeaderCell).Text("Is Reserved");
                    });

                    foreach (var reservation in reservations)
                    {
                        table.Cell().Element(Cell).Text(reservation.Id.ToString());
                        table.Cell().Element(Cell).Text(reservation.StartLocal.ToString("yyyy-MM-dd"));
                        table.Cell().Element(Cell).Text(reservation.EndLocal.ToString("yyyy-MM-dd"));
                        table.Cell().Element(Cell).Text(reservation.StudentFullName ?? "N/A");
                        table.Cell().Element(Cell).Text(reservation.IsReserved.ToString());
                    }

                    return;

                    static IContainer HeaderCell(IContainer c) =>
                        c.PaddingVertical(6).BorderBottom(1).DefaultTextStyle(TextStyle.Default.SemiBold());

                    static IContainer Cell(IContainer c) =>
                        c.EnsureSpace(8).PaddingVertical(2).BorderBottom(0.5f);
                });
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return pdfBytes.GeneratePdf();
    }
}