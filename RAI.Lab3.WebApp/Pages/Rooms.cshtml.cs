using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Pages;

[Authorize(Roles = AppRoles.Teacher)]
public class Rooms(IRoomService roomService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public List<RoomReadDto> RoomsList { get; set; } = [];
    
    [BindProperty]
    public Guid? EditingRoomId { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        await LoadRoomsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadRoomsAsync();
            return Page();
        }
        
        var roomDto = new RoomCreateUpdateDto
        {
            Name = Input.Name,
            Number = Input.Number
        };
        
        var result = await roomService.CreateRoomAsync(roomDto);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            await LoadRoomsAsync();
            return Page();
        }
        
        // Clear form and reload
        ModelState.Clear();
        Input = new();
        await LoadRoomsAsync();
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostEditAsync(Guid roomId)
    {
        EditingRoomId = roomId;
        await LoadRoomsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostCancelEditAsync()
    {
        EditingRoomId = null;
        await LoadRoomsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostUpdateAsync(Guid roomId, string name, int number)
    {
        var roomDto = new RoomCreateUpdateDto
        {
            Name = name,
            Number = number
        };
        
        var result = await roomService.UpdateRoomAsync(roomId, roomDto);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            EditingRoomId = roomId;
            await LoadRoomsAsync();
            return Page();
        }
        
        EditingRoomId = null;
        await LoadRoomsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(Guid roomId)
    {
        var result = await roomService.DeleteRoomAsync(roomId);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
        }
        
        await LoadRoomsAsync();
        return Page();
    }
    
    private async Task LoadRoomsAsync()
    {
        var result = await roomService.GetAllRoomsAsync();
        RoomsList = result.IsSuccess ? result.Value : new List<RoomReadDto>();
    }
    
    public class InputModel
    {
        [Required]
        [Display(Name = "Room Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Room Number")]
        [Range(1, 9999, ErrorMessage = "Room number must be between 1 and 9999")]
        public int Number { get; set; }
    }
}