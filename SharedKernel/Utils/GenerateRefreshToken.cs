using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Utils;

public static class GenerateRefreshToken
{
    public static string GenerateRefresh_Token(Int32 size)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(size));
    }
}
