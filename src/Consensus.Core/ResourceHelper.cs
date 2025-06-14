using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Consensus.Core;

internal static class ResourceHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetString(string resourceName)
    {
        var assembly = Assembly.GetCallingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
