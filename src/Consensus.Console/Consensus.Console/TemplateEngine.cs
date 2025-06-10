using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Consensus;

internal static class TemplateEngine
{
    private static readonly Regex TokenRegex = new(@"\$?{{\s*(?<token>[\w\.]+)\s*}}", RegexOptions.Compiled);

    public static string Render(string template, object values)
    {
        return TokenRegex.Replace(template, m =>
        {
            var token = m.Groups["token"].Value;
            var value = GetValue(values, token.Split('.'));
            return value?.ToString() ?? string.Empty;
        });
    }

    private static object? GetValue(object obj, IReadOnlyList<string> parts)
    {
        object? current = obj;
        foreach (var part in parts)
        {
            if (current is null)
            {
                return null;
            }
            if (current is IDictionary<string, object> dict)
            {
                dict.TryGetValue(part, out current);
            }
            else
            {
                var prop = current.GetType().GetProperty(part);
                if (prop is null)
                {
                    return null;
                }
                current = prop.GetValue(current);
            }
        }
        return current;
    }
}
