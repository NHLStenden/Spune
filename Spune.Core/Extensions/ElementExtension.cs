//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Spune.Core.Core;
using Spune.Core.Functions;

namespace Spune.Core.Extensions;

/// <summary>An Element extension class.</summary>
public static class ElementExtension
{
    /// <summary>
    /// Decodes the link by replacing placeholders with actual results from the running story.
    /// </summary>
    /// <param name="obj">The object containing the element.</param>
    /// <param name="runningStory">The running story containing the results to be used for placeholder replacement.</param>
    /// <returns>A string representing the decoded link.</returns>
    public static string DecodeLink(this Element obj, RunningStory runningStory)
    {
        var results = runningStory.Results.ToDictionary(x => x.Key, x => x.Value.Identifiers);
        var link = PlaceholderFunction.ReplacePlaceholders(obj.Link, results);
        return link;
    }
}