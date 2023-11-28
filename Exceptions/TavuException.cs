using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tavu.Exceptions
{
    public class TavuExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is TavuException tavuException)
            {
                context.Result = new ContentResult
                {
                    StatusCode = tavuException.ErrorCode,
                    Content = tavuException.SafeMessage,
                    ContentType  = "text/plain "
                };
                context.ExceptionHandled = true;
            }
        }
    }

    public class TavuException : Exception
    {
        public TavuException(string? message = null) : base(message)
        {

        }

        public string SafeMessage { get; protected set; } = "Internal server error.";
        public int ErrorCode { get; protected set; } = 500;
    }

    public class TavuServiceConfigurationException : TavuException
    {
        public TavuServiceConfigurationException(string? message = null) : base(message)
        {
        }
    }

    public class TavuUnauthorizedException : TavuException
    {
        public TavuUnauthorizedException(string? message = null)
            : base(message)
        {
            this.ErrorCode = 403;
            this.SafeMessage = "Unauthorized.";
        }
    }

    public class TavuValueValidationException : TavuException
    {
        public TavuValueValidationException(string? message = null) : base(message)
        {
            ErrorCode = 400;
            SafeMessage = "Invalid input parameters";
        }
    }
}