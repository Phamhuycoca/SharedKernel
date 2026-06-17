using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Utils;

public static class StringExtensions
{
    public static Dictionary<string, dynamic> ConvertSortObj(this string? sort)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        try
        {
            if (sort != null)
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(sort) ?? dictionary;
            }
        }
        catch (Exception)
        {
        }

        if (dictionary.Count != 0)
        {
            return dictionary;
        }

        dictionary.Add("id", 1);
        return dictionary;
    }

    public static dynamic ConvertFilterObj(this string? filter)
    {
        object result = null;
        try
        {
            if (filter != null)
            {
                result = JsonConvert.DeserializeObject<object>(filter);
            }
        }
        catch (Exception)
        {
        }

        return result;
    }
}