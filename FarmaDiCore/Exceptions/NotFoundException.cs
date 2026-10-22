namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción controlada para recursos solicitados que no existen en el sistema (HTTP 404).
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "El recurso solicitado no fue encontrado.", object? details = null)
            : base(message, 404, details)
        {
        }
    }
}
