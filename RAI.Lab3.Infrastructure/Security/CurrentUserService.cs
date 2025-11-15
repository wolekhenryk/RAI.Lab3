using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RAI.Lab3.Infrastructure.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId =>
        Guid.Parse(httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

    public string UserRole => httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}