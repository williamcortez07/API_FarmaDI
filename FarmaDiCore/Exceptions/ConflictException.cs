namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción controlada para conflictos con el estado actual del recurso (HTTP 409).
    /// Ejemplos: duplicidad de claves únicas, nombres de marcas o usuarios ya existentes, falta de inventario concurrente.
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message = "La operación generó un conflicto con los datos existentes.", object? details = null)
            : base(message, 409, details)
        {
        }
    }
}
