//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Extensions;

/// <summary>An IEnumerable&lt;T&gt; extension class.</summary>
public static class EnumerableExtension
{
    /// <summary>Finds the index of the first item matching an expression in an enumerable.</summary>
    /// <param name="obj">The object to perform method on.</param>
    /// <param name="predicate">The expression to test the items against.</param>
    /// <returns>The index of the first matching item, or -1 if no items match.</returns>
    /// <typeparam name="T">The type of item.</typeparam>
    public static int FindIndex<T>(this IEnumerable<T> obj, Func<T, bool> predicate)
    {
        var result = 0;
        foreach (var item in obj)
        {
            if (predicate(item))
                return result;
            result++;
        }

        return -1;
    }
}