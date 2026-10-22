namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción controlada para datos de entrada inválidos o incumplimiento de invariantes de negocio (HTTP 400).
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(string message = "Los datos suministrados no son válidos.", object? details = null)
            : base(message, 400, details)
        {
        }
    }
}
