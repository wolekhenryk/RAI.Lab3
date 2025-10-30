using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RAI.Lab3.Infrastructure.Security;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var test = Guid.Parse(httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            
            return test;
        }
    }

    public string UserRole => httpContextAccessor.HttpContext?.User
        .FindFirst("role")?.Value ?? string.Empty;
}