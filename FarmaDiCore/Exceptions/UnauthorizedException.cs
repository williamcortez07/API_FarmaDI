namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción controlada para solicitudes con credenciales faltantes o inválidas (HTTP 401).
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Credenciales de autenticación no válidas.", object? details = null)
            : base(message, 401, details)
        {
        }
    }
}
