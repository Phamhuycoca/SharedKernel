using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Settings;

public class JwtSetting
{
    /// <summary>
    /// Issuer: ai phát token (AuthService)
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// Audience: ai sẽ nhận token (Client App)
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// Khóa bí mật dùng để sign token
    /// Nên >= 32 ký tự, random, giữ bí mật
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Thời gian sống Access Token (phút)
    /// Access Token nên ngắn hạn (10-15 phút)
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>
    /// Thời gian sống Refresh Token (ngày)
    /// Refresh Token dài hạn, rotate token
    /// </summary>
    public int RefreshTokenDays { get; set; } = 7;
}
