using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrapper.Response;

public class ResponseDto<T>
{
    public T? data { get; set; }
    public string? message { get; set; }
    public bool success { get; set; } = true;
    public int status_code { get; set; } = 200;
    public ResponseDto()
    {
    }
    public ResponseDto(T _data, string _message = "Thành công", bool _success = true, int _status_code = 200)
    {
        data = _data;
        message = _message;
        success = _success;
        status_code = _status_code;
    }
}