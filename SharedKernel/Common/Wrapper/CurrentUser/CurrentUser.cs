using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrapper.CurrentUser;

public class CurrentUser : ICurrentUser
{
    protected readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? Browser
    {
        get
        {
            var ua = UserAgent;

            if (string.IsNullOrEmpty(ua))
                return null;

            if (ua.Contains("Chrome"))
                return "Chrome";

            if (ua.Contains("Firefox"))
                return "Firefox";

            if (ua.Contains("Edg"))
                return "Edge";

            return ua;
        }
    }

    public string? browser => throw new NotImplementedException();

    public string? ip_address
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            var ip = context?.Connection?.RemoteIpAddress?.ToString();

            return ip;
        }
    }

    public string? token
    {
        get
        {
            var authHeader = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .ToString();

            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            return authHeader.Replace("Bearer ", "");
        }
    }

    public string? user_id => throw new NotImplementedException();

    public bool is_authenticated => throw new NotImplementedException();

    private string? UserAgent => _httpContextAccessor.HttpContext?
      .Request.Headers["User-Agent"]
      .ToString();
}
