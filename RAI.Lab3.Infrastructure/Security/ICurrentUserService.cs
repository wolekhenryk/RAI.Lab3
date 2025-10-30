namespace RAI.Lab3.Infrastructure.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserRole { get; }
}