//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Functions;

/// <summary>This class contains a collection of parser functions.</summary>
public static class ParserFunction
{
	/// <summary>
	/// Splits a string with C# parameters to individual values.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The List&lt;string&gt;.</returns>
	public static IList<string> CSharpParameterSplit(string s)
    {
        var trimmed = s.Trim();
        var result = new List<string>();
        const char separatorChar = ',';
        const char quoteChar = '\"';
        const char escapeChar = '\\';

        var begin = FirstNonWhiteSpace(trimmed, 0);
        if (begin < 0) return [];
        var i = begin;
        while (i < trimmed.Length)
        {
            if (IsWhiteSpace(trimmed[i]))
            {
                i++;
                i = FirstNonWhiteSpace(trimmed, i);
                // Invalid string
                if (i < 0)
                    return [];
            }
            else if (trimmed[i] == separatorChar)
            {
                if (i > begin)
                    result.Add(trimmed[begin..i].Trim());
                i++;
                begin = i;
            }
            else if (trimmed[i] == quoteChar)
            {
                i++;
                i = FirstQuote(trimmed, quoteChar, escapeChar, i);
                // Invalid string
                if (i < 0)
                    return [];
                i++;
            }
            else
            {
                i++;
            }
        }

        if (i >= trimmed.Length && begin < i)
            result.Add(trimmed[begin..].Trim());

        return result;
    }

	/// <summary>
	/// Gets the index of the first quote char.
	/// </summary>
	/// <param name="s">String to parse.</param>
	/// <param name="quoteChar">(Optional) The quote character.</param>
	/// <param name="escapeChar">(Optional) The escape character.</param>
	/// <param name="startIndex">Start index.</param>
	/// <returns>The index or -1 otherwise.</returns>
	static int FirstQuote(string s, char quoteChar, char escapeChar, int startIndex)
    {
        var i = startIndex;
        while (i < s.Length)
        {
            if (s[i] == escapeChar)
                i += 2;
            else if (s[i] == quoteChar)
                break;
            else
                i++;
        }

        return i < s.Length ? i : -1;
    }

	/// <summary>
	/// Gets the index of the first non-whitespace.
	/// </summary>
	/// <param name="s">String to parse.</param>
	/// <param name="startIndex">Start index.</param>
	/// <returns>The index or -1 otherwise.</returns>
	static int FirstNonWhiteSpace(string s, int startIndex)
    {
        for (var i = startIndex; i < s.Length; i++)
        {
            if (!IsWhiteSpace(s[i]))
                return i;
        }

        return -1;
    }

	/// <summary>
	/// Query if 'c' is a whitespace.
	/// </summary>
	/// <param name="c">The character.</param>
	/// <returns>True if it is a whitespace, false if not.</returns>
	static bool IsWhiteSpace(char c)
    {
        const string set = " \r\n\t";
        return set.Contains(c);
    }
}