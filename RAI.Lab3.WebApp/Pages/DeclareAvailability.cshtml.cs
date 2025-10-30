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
        
        return Page();
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        await LoadRoomsAsync();
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
}