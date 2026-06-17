using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrapper.Exceptions;

public class ApiErrorResponse
{
    public int status_code { get; set; }
    public string message { get; set; } = string.Empty;
    public string trace_id { get; set; } = string.Empty;
    public DateTime time_stamp { get; set; } = DateTime.UtcNow;
    public object? errors { get; set; }
    public string? instance { get; set; }
}