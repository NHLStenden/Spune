//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Common.Extensions;

/// <summary>An ICollection&lt;T&gt; extension class.</summary>
public static class CollectionExtension
{
    /// <summary>
    /// Removes the element at the specified index of the collection.
    /// </summary>
    /// <param name="obj">The object to perform method on.</param>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <typeparam name="T">The type of item.</typeparam>
    public static void RemoveAt<T>(this ICollection<T> obj, int index)
    {
        var result = 0;
        foreach (var item in obj)
        {
            if (result == index)
            {
                obj.Remove(item);
                return;
            }

            result++;
        }
    }
}