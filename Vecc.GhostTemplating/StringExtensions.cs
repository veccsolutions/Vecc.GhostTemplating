using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vecc.GhostTemplating
{
    public static class StringExtensions
    {
        public static string EscapeForJson(this string value)
        {
            var result = JsonSerializer.Serialize(value);

            return result;
        }
    }
}
