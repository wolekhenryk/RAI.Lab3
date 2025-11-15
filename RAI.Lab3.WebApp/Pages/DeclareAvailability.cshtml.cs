using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Pages;

[Authorize(Roles = AppRoles.Teacher)]
public class DeclareAvailability(
    IAvailabilityService availabilityService,
    IReservationService reservationService,
    IRoomService roomService) : PageModel
{
    [BindProperty]
    public TeacherAvailabilityCreateDto Input { get; set; } = new();
    public List<SelectListItem> AvailableRooms { get; set; } = [];
    
    public List<TeacherAvailabilityReadDto> ExistingAvailabilities { get; set; } = [];

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await availabilityService.CreateAvailabilityAsync(Input);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            await LoadRoomsAsync();
            return Page();
        }
        
        ModelState.Clear();
        Input = new TeacherAvailabilityCreateDto();
        await LoadRoomsAsync();
        await LoadExistingAvailabilitiesAsync();
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        await LoadRoomsAsync();
        await LoadExistingAvailabilitiesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteSlotAsync(Guid slotId)
    {
        var deleteResult = await reservationService.DeleteReservationAsync(slotId);
        if (!deleteResult.IsSuccess)
            ModelState.AddModelError(string.Empty, deleteResult.Error?.Message ?? "An error occurred while deleting the reservation.");
        
        await LoadRoomsAsync();
        await LoadExistingAvailabilitiesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostBlockAvailabilityAsync(Guid availabilityId)
    {
        var blockResult = await availabilityService.BlockAvailabilityAsync(availabilityId);
        if (!blockResult.IsSuccess)
            ModelState.AddModelError(string.Empty, blockResult.Error?.Message ?? "An error occurred while blocking the availability.");
        
        await LoadRoomsAsync();
        await LoadExistingAvailabilitiesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUnblockAvailabilityAsync(Guid availabilityId)
    {
        var unblockResult = await availabilityService.UnblockAvailabilityAsync(availabilityId);
        if (!unblockResult.IsSuccess)
            ModelState.AddModelError(string.Empty, unblockResult.Error?.Message ?? "An error occurred while unblocking the availability.");
        
        await LoadRoomsAsync();
        await LoadExistingAvailabilitiesAsync();
        return Page();
    }

    private async Task LoadRoomsAsync()
    {
        var roomsResult = await roomService.GetAllRoomsAsync();
        if (roomsResult.IsSuccess)
        {
            AvailableRooms = roomsResult.Value
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.Name} (Room {r.Number})" })
                .ToList();
        }
    }
    
    private async Task LoadExistingAvailabilitiesAsync()
    {
        var availabilitiesResult = await availabilityService.GetAllAvailabilitiesAsync();
        if (availabilitiesResult.IsSuccess)
        {
            ExistingAvailabilities = availabilitiesResult.Value;
        }
    }
}