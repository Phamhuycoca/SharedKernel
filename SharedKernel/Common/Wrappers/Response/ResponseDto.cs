using SharedKernel.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrappers.Response;

public class ResponseDto<T>
{
    public T? data { get; set; }
    public string? message { get; set; }
    public bool success { get; set; }
    public int status_code { get; set; }
    public ResponseDto()
    {
    }
    public ResponseDto(T _data, string _message, bool _success, int _status_code)
    {
        data = _data;
        message = _message;
        success = _success;
        status_code = _status_code;
    }
}
