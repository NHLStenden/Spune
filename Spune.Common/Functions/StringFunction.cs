//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text;

namespace Spune.Common.Functions;

/// <summary>
/// This class contains string functions.
/// </summary>
public static class StringFunction
{
    /// <summary>
    /// Duplicate the specified string a given number of times.
    /// </summary>
    /// <returns>The duplicate.</returns>
    /// <param name="s">String.</param>
    /// <param name="times">Times.</param>
    public static List<string?> Duplicate(string? s, int times)
    {
        var result = new List<string?>();
        for (var i = 0; i < times; i++) result.Add(s);
        return result;
    }

    /// <summary>
    /// Converts a list of strings to 1 string using the new line as a separator.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="sl">The list.</param>
    public static string StringsToString(IEnumerable<string?> sl) => StringsToString(sl, Environment.NewLine);

    /// <summary>
    /// Converts a list of strings to 1 string using the given terminator.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="list">The list.</param>
    /// <param name="terminator">Terminator.</param>
    public static string StringsToString(IEnumerable<string?> list, string terminator)
    {
        var result = new StringBuilder();
        using var enumerator = list.GetEnumerator();
        var i = 0;
        while (enumerator.MoveNext())
        {
            var value = enumerator.Current;
            if (value == null) continue;
            if (i > 0)
                result.Append(terminator);
            result.Append(value);
            i++;
        }

        return result.ToString();
    }
}