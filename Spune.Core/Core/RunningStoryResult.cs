//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Core.Core;

/// <summary>
/// Represents the results of a story, including both text results and identifier results.
/// </summary>
public class RunningStoryResult
{
    /// <summary>
    /// Gets or sets the list of text results associated with the story.
    /// </summary>
    /// <value>
    /// A list of strings representing the text results.
    /// </value>
    public List<string> Texts { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of identifier results associated with the story.
    /// </summary>
    /// <value>
    /// A list of strings representing the identifier results.
    /// </value>
    public List<string> Identifiers { get; set; } = [];
}
