using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LyricsFinder
{
    internal static class JsonExtension
    {
        public static T GetValue<T>(this JsonElement cur, params string[] paths)
        {
            foreach (var path in paths)
            {
                if (!cur.TryGetProperty(path, out var next))
                    return default;

                cur = next;
            }

            var rawText = cur.GetRawText();
            return JsonSerializer.Deserialize<T>(rawText);
        }

        public static T GetValue<T>(this JsonDocument json, params string[] paths) => json.RootElement.GetValue<T>(paths);
    }
}
