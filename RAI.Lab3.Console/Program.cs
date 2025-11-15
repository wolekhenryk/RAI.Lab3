using System.Net.Http.Headers;
using System.Net.Http.Json;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Console;

using var client = new HttpClient();

client.BaseAddress = new Uri("http://localhost:5273");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

Console.WriteLine("Enter your email:");
var email = Console.ReadLine();
Console.WriteLine("Enter your password:");
var password = Console.ReadLine();

var response = await client.PostAsJsonAsync("/api/illegal-login", new IllegalUserLoginDto(email, password));
var jwtResponse = await response.Content.ReadFromJsonAsync<JwtResponse>();
var jwt = jwtResponse!.Token;

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

Console.WriteLine("1: Create Reservation Illegally, 2: See available slots");
var choice = Console.ReadLine();
if (choice == "1")
{
    var reservationId = Guid.TryParse(Console.ReadLine(), out var resId) 
        ? resId 
        : throw new Exception("Invalid GUID");

    var illegalReservationResponse = await client.PostAsync($"/api/slots/{reservationId}/book", null);
    if (!illegalReservationResponse.IsSuccessStatusCode)
    {
        var errorMessage = await illegalReservationResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {errorMessage}");
    }
    else
    {
        Console.WriteLine("Reservation created successfully!");
    }
}
else if (choice == "2")
{
    var availableSlotsResponse = await client.GetAsync("/api/slots/available");
    if (!availableSlotsResponse.IsSuccessStatusCode)
    {
        var errorMessage = await availableSlotsResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {errorMessage}");
    }
    else
    {
        var slots = await availableSlotsResponse.Content.ReadFromJsonAsync<List<TeacherAvailabilityReadDto>>();
        foreach (var slot in slots!)
        {
            Console.WriteLine($"Slot ID: {slot.Id}, Teacher: {slot.TeacherFullName}, Room: {slot.RoomName}, Start: {slot.StartDate} {slot.StartTime}, End: {slot.EndDate} {slot.EndTime}, IsBlocked: {slot.IsBlocked}");
            foreach (var res in slot.Reservations)
            {
                Console.WriteLine($"\tReservation ID: {res.Id}, Student: {res.StudentFullName}, StartLocal: {res.StartLocal}, EndLocal: {res.EndLocal}");
            }
        }
    }
}