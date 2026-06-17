using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Utils;

public static class PasswordHelper
{
    public static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}