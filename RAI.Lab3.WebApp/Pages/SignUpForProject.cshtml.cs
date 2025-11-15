using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Pages;

[Authorize(Roles = AppRoles.Student)]
public class SignUpForProject(
    IAvailabilityService availabilityService,
    IReservationService reservationService) : PageModel
{
    public List<TeacherAvailabilityReadDto> AvailableSlots { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAvailableSlotsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSignUpAsync(Guid slotId)
    {
        var result = await reservationService.CreateReservationAsync(slotId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
        }
        else
        {
            TempData["SuccessMessage"] = "Successfully signed up for the slot!";
        }

        await LoadAvailableSlotsAsync();
        return Page();
    }

    private async Task LoadAvailableSlotsAsync()
    {
        var availabilitiesResult = await availabilityService.GetAllAvailabilitiesAsync();
        if (availabilitiesResult.IsSuccess)
        {
            // Filter to only show future slots that are not blocked
            AvailableSlots = availabilitiesResult.Value
                .Where(a => !a.IsBlocked)
                .Select(a => new TeacherAvailabilityReadDto
                {
                    Id = a.Id,
                    TeacherFullName = a.TeacherFullName,
                    RoomName = a.RoomName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    IsBlocked = a.IsBlocked,
                    Reservations = a.Reservations
                        .Where(r => r.StartLocal > DateTime.Now)
                        .ToList()
                })
                .Where(a => a.Reservations.Any())
                .ToList();
        }
    }
}