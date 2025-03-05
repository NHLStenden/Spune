//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Spune.Common.Functions;

namespace Spune.Core.Functions;

/// <summary>
/// This class contains placeholder related functions.
/// </summary>
public static class PlaceholderFunction
{
    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    public static string ReplacePlaceholders(string text, IDictionary<string, List<string>> dictionary)
    {
        return ReplacePlaceholders(text, dictionary, string.Empty);
    }
    
    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <param name="keyPrefix">A prefix for the key identifier.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    public static string ReplacePlaceholders(string text, IDictionary<string, string> dictionary, string keyPrefix)
    {
        const string doubleLeftBrace = @"\{\{";
        const string doubleRightBrace = @"\}\}";
        // Escape literal double curly braces
        var result = text.Replace("{{{{", doubleLeftBrace, StringComparison.Ordinal);
        result = result.Replace("}}}}", doubleRightBrace, StringComparison.Ordinal);

        // Replace placeholders with values
        foreach (var (key, value) in dictionary)
        {
            var placeholder = !string.IsNullOrEmpty(keyPrefix) ? "{{" + keyPrefix + key + "}}" : "{{" + key + "}}";
            result = result.Replace(placeholder, value, StringComparison.Ordinal);
        }

        // Unescape literal double curly braces
        result = result.Replace(doubleLeftBrace, "{{", StringComparison.Ordinal);
        result = result.Replace(doubleRightBrace, "}}", StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Replaces placeholders in a string with corresponding values from a dictionary.
    /// </summary>
    /// <param name="text">The string containing placeholders.</param>
    /// <param name="dictionary">A dictionary with keys as placeholder names and values as replacement values.</param>
    /// <param name="keyPrefix">A prefix for the key identifier.</param>
    /// <returns>The string with placeholders replaced by their corresponding values.</returns>
    static string ReplacePlaceholders(string text, IDictionary<string, List<string>> dictionary, string keyPrefix)
    {
        var newDictionary = new Dictionary<string, string>();
        foreach (var (key, value) in dictionary)
            newDictionary[key] = StringFunction.StringsToString(value, ", ");
        return ReplacePlaceholders(text, newDictionary, keyPrefix);
    }
}