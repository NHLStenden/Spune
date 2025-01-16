//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Functions;

/// <summary>
/// This class contains string methods for post-processing.
/// </summary>
public static class PostProcessFunction
{
    /// <summary>
    /// Makes the first character of the given object lower case.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The changed input string.</returns>
    public static string LowerCaseFirstChar(string input) => !string.IsNullOrEmpty(input) ? char.ToLower(input[0]) + input[1..] : input;

    /// <summary>
    /// Makes the first character of the given object upper case.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The changed input string.</returns>
    public static string UpperCaseFirstChar(string input) => !string.IsNullOrEmpty(input) ? char.ToUpper(input[0]) + input[1..] : input;

    /// <summary>
    /// Trims the given object.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The changed input string.</returns>
    public static string Trim(string input) => input.Trim();
}