//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.UIShared.Core;

/// <summary>
/// Represents the categories of interactions available for user input or system behavior.
/// </summary>
/// <remarks>
/// This enumeration is used to define various interaction modalities within the application.
/// It helps to classify input methods such as selecting a single option, multiple options,
/// or providing a free-text response to accommodate different interaction requirements.
/// </remarks>
public enum InteractionType
{
    /// <summary>
    /// Denotes an interaction type where a user is required to select a single option from a list of predefined choices.
    /// </summary>
    SingleSelect,

    /// <summary>
    /// Represents a multi-select interaction type where the user can choose multiple options from a predefined set.
    /// </summary>
    MultiSelect,

    /// <summary>
    /// Represents an open-ended question interaction type where the user is allowed to provide free-form responses without
    /// predefined constraints.
    /// </summary>
    OpenQuestion
}