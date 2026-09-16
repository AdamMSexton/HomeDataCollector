namespace HealthDataCollector.Constants;

public static class ServiceMessages
{
    public const string ApiKeyNotFound = "Invalid API key.";
    public const string ApiKeyExpired = "API key has expired.";
    public const string ApiKeyRevoked = "API key has been revoked.";
    public const string ApiKeyRenewed = "API key was renewed.";
    public const string ApiKeyTooEarlyRenew = "API key not renewable (early).";
    public const string ImproperFormat = "Improper impot received.";
    public const string InternalError = "An internal error occurred.";
    public const string SourceNotFound = "SourceId was not found.";
    
}