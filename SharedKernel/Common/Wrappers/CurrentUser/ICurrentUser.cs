using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrappers.CurrentUser;

public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    string? Browser { get; }
    string? IpAddress { get; }
    string? Token { get; }
}
