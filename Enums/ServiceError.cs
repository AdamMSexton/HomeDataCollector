namespace HealthDataCollector.Enums
{
    public enum ServiceError
    {
        None,
        NotFound,
        NotAllowed,
        Validation,
        Conflict,
        Database,
        Internal,

        ApiKeyNotFound,
        ApiKeyExpired,
        ApiKeyOutsideRenewalWin,
        ApiKeyRevoked,
        ApiKeyRenewed

    }
}
