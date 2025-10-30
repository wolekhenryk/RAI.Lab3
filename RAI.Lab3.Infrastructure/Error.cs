namespace RAI.Lab3.Infrastructure;

public sealed record Error(string Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}