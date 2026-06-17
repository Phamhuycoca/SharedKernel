using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrapper.CurrentUser;

public interface ICurrentUser
{
    string? user_id { get; }
    bool is_authenticated { get; }
    string? browser { get; }
    string? ip_address { get; }
    string? token { get; }
}