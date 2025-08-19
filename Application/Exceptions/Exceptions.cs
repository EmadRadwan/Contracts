namespace API.Middleware;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }
    public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class ServiceException : ApplicationException
{
    public ServiceException(string message, Exception innerException = null) : base(message, innerException) { }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message) : base(message) { }
}

public class ConcurrencyException : ApplicationException
{
    public ConcurrencyException(string message) : base(message) { }
}

public class OperationFailedException : ApplicationException
{
    public OperationFailedException(string message) : base(message) { }
}

public class BusinessRuleViolationException : ApplicationException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}

public class DataAccessException : ApplicationException
{
    public DataAccessException(string message, Exception innerException = null) : base(message, innerException) { }
}

public class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(string entityName, string key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

public class DuplicateRecordException : DataAccessException
{
    public DuplicateRecordException(string message) : base(message) { }
}

public class ForeignKeyViolationException : DataAccessException
{
    public ForeignKeyViolationException(string message) : base(message) { }
}

public class UniqueConstraintViolationException : DataAccessException
{
    public UniqueConstraintViolationException(string message) : base(message) { }
}

public class DatabaseTimeoutException : DataAccessException
{
    public DatabaseTimeoutException(string message) : base(message) { }
}

public class TransactionFailedException : DataAccessException
{
    public TransactionFailedException(string message, Exception innerException = null) : base(message, innerException) { }
}

public class InvalidInputException : ValidationException
{
    public InvalidInputException(IDictionary<string, string[]> errors)
        : base(errors) { }
}

public class MissingRequiredFieldException : ValidationException
{
    public MissingRequiredFieldException(string fieldName)
        : base(new Dictionary<string, string[]> { { fieldName, new[] { $"{fieldName} is required." } } }) { }
}

public class RangeViolationException : ValidationException
{
    public RangeViolationException(string fieldName, string message)
        : base(new Dictionary<string, string[]> { { fieldName, new[] { message } } }) { }
}

public class FormatException : ValidationException
{
    public FormatException(string fieldName, string message)
        : base(new Dictionary<string, string[]> { { fieldName, new[] { message } } }) { }
}

public class InvalidOperationException : ApplicationException
{
    public InvalidOperationException(string message) : base(message) { }
}

public class ExternalServiceException : ApplicationException
{
    public ExternalServiceException(string message, Exception innerException = null) : base(message, innerException) { }
}

public class ApiRequestFailedException : ExternalServiceException
{
    public ApiRequestFailedException(string api, string message)
        : base($"API request to {api} failed: {message}") { }
}

public class TimeoutException : ExternalServiceException
{
    public TimeoutException(string service)
        : base($"The request to service {service} timed out.") { }
}

public class AuthenticationFailedException : ExternalServiceException
{
    public AuthenticationFailedException(string service)
        : base($"Authentication with service {service} failed.") { }
}

public class RateLimitExceededException : ExternalServiceException
{
    public RateLimitExceededException(string service)
        : base($"Rate limit exceeded for service {service}.") { }
}

public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message) : base(message) { }
}

public class TokenExpiredException : AuthenticationException
{
    public TokenExpiredException() : base("Authentication token has expired.") { }
}

public class PermissionDeniedException : ApplicationException
{
    public PermissionDeniedException(string action)
        : base($"You do not have permission to perform the action: {action}") { }
}

public class SecurityBreachException : ApplicationException
{
    public SecurityBreachException(string message) : base(message) { }
}

public class FileNotFoundException : ApplicationException
{
    public FileNotFoundException(string filePath) : base($"File not found at path: {filePath}") { }
}

public class FileReadException : ApplicationException
{
    public FileReadException(string filePath) : base($"Failed to read the file at path: {filePath}") { }
}

public class FileWriteException : ApplicationException
{
    public FileWriteException(string filePath) : base($"Failed to write to the file at path: {filePath}") { }
}

public class FilePermissionException : ApplicationException
{
    public FilePermissionException(string filePath)
        : base($"Insufficient permissions to access the file at path: {filePath}") { }
}

public class StorageQuotaExceededException : ApplicationException
{
    public StorageQuotaExceededException(string userId)
        : base($"User {userId} exceeded their storage quota.") { }
}

public class ConfigurationException : ApplicationException
{
    public ConfigurationException(string message) : base(message) { }
}

public class DependencyFailureException : ApplicationException
{
    public DependencyFailureException(string dependency)
        : base($"Failed due to a dependency on {dependency}.") { }
}

public class UnsupportedOperationException : ApplicationException
{
    public UnsupportedOperationException(string operation)
        : base($"The operation '{operation}' is not supported.") { }
}




