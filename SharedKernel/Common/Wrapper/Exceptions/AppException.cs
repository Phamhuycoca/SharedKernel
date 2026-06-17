using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrapper.Exceptions;

public class AppException : Exception
{
    public HttpStatusCode status_code { get; }
    public string? detail { get; }

    public AppException(HttpStatusCode _status_code,string message,string? _detail = null): base(message)
    {
        status_code = _status_code;
        detail = _detail;
    }
    public AppException(string message, string? _detail = null) : base(message)
    {
        detail = _detail;
    }
}
