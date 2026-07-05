using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrappers.ErrorResponse;

public class ApiErrorResponse
{
    public int status_code { get; set; }
    public string message { get; set; } = string.Empty;
    public string trace_id { get; set; } = string.Empty;
    public DateTime time_stamp { get; set; }    
    public object? errors { get; set; }
    public string? instance { get; set; }
    public ApiErrorResponse()
    {
    }
    public ApiErrorResponse(int _status_code, string _message, string _trace_id, object? _errors = null, string? _instance = null)
    {
        status_code = _status_code;
        message = _message;
        trace_id = _trace_id;
        errors = _errors;
        instance = _instance;
        time_stamp = DateTime.UtcNow;
    }
}
