//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection;

namespace Spune.Common.Miscellaneous;

/// <summary>This class offers general dotnet functionality.</summary>
public static class DotNetHelper
{
    /// <summary>
    /// Check if this platform is the browser.
    /// </summary>
    static readonly bool IsOnBrowser = OperatingSystem.IsBrowser();

    /// <summary>
    /// Get the application file name.
    /// </summary>
    /// <returns>The application file name or "" otherwise.</returns>
    public static string GetApplicationFileName()
    {
        if (IsOnBrowser)
            return "";
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule?.FileName != null)
        {
            var fileName = processModule.FileName;
            if (!fileName.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase))
                return fileName;
        }

        var assembly = GetApplicationAssembly();
        if (assembly == null)
            return "";
#pragma warning disable IL3000
        var result = assembly.Location;
#pragma warning restore IL3000
        if (string.IsNullOrEmpty(result)) result = AppContext.BaseDirectory;
        return new Uri(result).LocalPath;
    }

    /// <summary>
    /// Gets the application assembly.
    /// </summary>
    /// <returns>The assembly or null otherwise.</returns>
    static Assembly? GetApplicationAssembly() => Assembly.GetEntryAssembly();
}