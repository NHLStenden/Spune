//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Extensions;

/// <summary>An IList&lt;T&gt; extension class.</summary>
public static class ListExtension
{
    /// <summary>
    /// Random member.
    /// </summary>
    static readonly Random Random = new();

    /// <summary>
    /// Shuffles the elements of the given list in place using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to be shuffled.</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}