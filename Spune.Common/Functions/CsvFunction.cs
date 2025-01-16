//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Functions;

/// <summary>This class contains general csv-string functions.</summary>
public static class CsvFunction
{
    /// <summary>
    /// Converts a value to a comma-separated values field: format it is necessary.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted string or the original string otherwise.</returns>
    public static string ValueToCsvField(string value) => string.IsNullOrEmpty(value) || !NeedsDoubleQuotes(value) ? value : QuotedString(DoubleTheQuotes(value, '\"'), '\"');

    /// <summary>
    /// Double the double quotes in a string. So: turn "Hello" into ""Hello"".
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="quoteChar">The quote character.</param>
    /// <returns>A string.</returns>
    static string DoubleTheQuotes(string value, char quoteChar) => string.IsNullOrEmpty(value) ? value : value.Aggregate("", (current, c) => current + (c == quoteChar ? Invariant($"{quoteChar}{quoteChar}") : c.ToString()));

    /// <summary>
    /// Checks if a string (a field) needs double quotes.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>True if needs it, false if it doesn't.</returns>
    static bool NeedsDoubleQuotes(string value) => value.Any(c => c is ',' or '\"') || value.Contains("\r\n") || value.Contains('\n');

    /// <summary>
    /// Returns a quoted string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="quote">The quote.</param>
    /// <returns>A string.</returns>
    static string QuotedString(string value, char quote) => quote + value + quote;
}