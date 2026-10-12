namespace HomeDataCollector.Enums
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
