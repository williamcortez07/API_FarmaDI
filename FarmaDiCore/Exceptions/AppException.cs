using System;

namespace FarmaDiCore.Exceptions
{
    /// <summary>
    /// Excepción base para todas las excepciones controladas del dominio/aplicación.
    /// Permite asignar un código HTTP semántico y detalles sin exponer información sensible de infraestructura.
    /// </summary>
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        public object? Details { get; }

        protected AppException(string message, int statusCode = 500, object? details = null)
            : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }

        protected AppException(string message, Exception innerException, int statusCode = 500, object? details = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }
}
