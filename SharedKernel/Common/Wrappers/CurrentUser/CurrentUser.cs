using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrappers.CurrentUser;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUser(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? UserId =>
        _contextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsAuthenticated =>
        _contextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? Browser =>
        _contextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public string? IpAddress =>
        _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? Token =>
        _contextAccessor.HttpContext?
            .Request.Headers.Authorization.ToString()
            .Replace("Bearer ", "");
}
