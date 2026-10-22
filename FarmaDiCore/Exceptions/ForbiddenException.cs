namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción controlada para usuarios autenticados que carecen de permisos suficientes sobre el recurso (HTTP 403).
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "No posee los privilegios requeridos para ejecutar esta acción.", object? details = null)
            : base(message, 403, details)
        {
        }
    }
}
