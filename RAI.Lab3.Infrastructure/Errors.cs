using Npgsql;

namespace RAI.Lab3.Infrastructure;

public static class Errors
{
    public static class Db
    {
        public static Error UniqueViolation(string? message = null) =>
            new("db.unique_violation", message ?? "Unique constraint violated.");

        public static Error ForeignKeyViolation(string? message = null) =>
            new("db.foreign_key_violation", message ?? "Foreign key constraint violated.");

        public static Error Timeout(string? message = null) =>
            new("db.timeout", message ?? "Database operation timed out.");

        public static Error NotFound(string? message = null) =>
            new("db.not_found", message ?? "Entity not found.");

        public static Error UnknownError(string? message = null) =>
            new("db.unknown_error", message ?? "An unknown database error occurred.");
    }
}

public static class ErrorHelpers
{
    public static Error MatchToPostgresException(this PostgresException exception, string? message = null)
    {
        return exception.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => Errors.Db.UniqueViolation(message ?? exception.MessageText),
            PostgresErrorCodes.ForeignKeyViolation => Errors.Db.ForeignKeyViolation(message ?? exception.MessageText),
            PostgresErrorCodes.QueryCanceled => Errors.Db.Timeout(message ?? exception.MessageText),
            _ => Errors.Db.UnknownError(message ?? exception.MessageText)
        };
    }
}