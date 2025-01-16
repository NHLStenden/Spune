//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
using Spune.Common.Miscellaneous;

namespace Spune.UIShared.Core;

/// <summary>
/// Represents an interaction within a system, extending SubElement functionality.
/// Provides properties and configuration for user interactions such as choices
/// and prompts within a storyline or context.
/// </summary>
/// <remarks>
/// The Interaction class is used to represent user or system-driven interactions,
/// specifying attributes like whether the interaction is mandatory, its relation
/// to the story progression, an associated link, a prompt for the user, and the
/// type of interaction. This class provides the necessary details to define,
/// associate, or navigate interactions in applications such as story-driven
/// environments or interactive systems.
/// </remarks>
public class Interaction : SubElement
{
    /// <summary>
    /// The height member.
    /// </summary>
    double _height;

    /// <summary>
    /// The Post-processing member.
    /// </summary>
    string _postProcessing = string.Empty;

    /// <summary>
    /// The prompt member.
    /// </summary>
    string _prompt = string.Empty;

    /// <summary>
    /// Remove after use member.
    /// </summary>
    bool _removeAfterUse;

    /// <summary>
    /// Is result member.
    /// </summary>
    bool _setsResult;

    /// <summary>
    /// The text is visible member.
    /// </summary>
    bool _textIsVisible;

    /// <summary>
    /// The type member.
    /// </summary>
    InteractionType _type;

    /// <summary>
    /// The width member.
    /// </summary>
    double _width;

    /// <summary>
    /// The X position member.
    /// </summary>
    double _xPosition;

    /// <summary>
    /// The Y position member.
    /// </summary>
    double _yPosition;

    /// <summary>
    /// Represents the height (fraction) in the image.
    /// </summary>
    public double Height
    {
        get => _height;
        set
        {
            _height = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the post-processing content associated with the element.
    /// </summary>
    public string PostProcessing
    {
        get => _postProcessing;
        set
        {
            _postProcessing = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents a prompt associated with an interaction.
    /// The prompt serves as the main textual content or question that guides the user within the interaction.
    /// </summary>
    /// <remarks>
    /// This property typically contains descriptive or instructional text displayed to the user during an interaction.
    /// The content of the prompt can vary depending on the interaction type, such as a choice question, multi-selection,
    /// or open-ended question.
    /// </remarks>
    public string Prompt
    {
        get => _prompt;
        set
        {
            _prompt = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether the interaction is removed after it has been used.
    /// </summary>
    public bool RemoveAfterUse
    {
        get => _removeAfterUse;
        set
        {
            _removeAfterUse = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether the interaction sets the result of the chapter.
    /// </summary>
    public bool SetsResult
    {
        get => _setsResult;
        set
        {
            _setsResult = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the text content associated with the element.
    /// </summary>
    public bool TextIsVisible
    {
        get => _textIsVisible;
        set
        {
            _textIsVisible = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the type of interaction associated with an <see cref="Interaction" /> instance.
    /// Defines how the interaction will be presented and handled, such as choice-based,
    /// multi-selection, or open-ended input.
    /// </summary>
    public InteractionType Type
    {
        get => _type;
        set
        {
            _type = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the width (fraction) in the image.
    /// </summary>
    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the X position (fraction) in the image.
    /// </summary>
    public double XPosition
    {
        get => _xPosition;
        set
        {
            _xPosition = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the Y position (fraction) in the image.
    /// </summary>
    public double YPosition
    {
        get => _yPosition;
        set
        {
            _yPosition = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Determines whether the element contains post-processing.
    /// </summary>
    /// <returns>True if has it, and false otherwise.</returns>
    public bool HasPostProcessing() => !string.IsNullOrEmpty(PostProcessing);

    /// <summary>
    /// Indicates whether the interaction is in the image or separately.
    /// </summary>
    public bool InImage() => _width > 0.0 && _height > 0.0;

    /// <summary>
    /// Post-processing the given string with the post-processing operations specified in property PostProcessing.
    /// </summary>
    /// <param name="text">Text to handle.</param>
    /// <returns>The processed text.</returns>
    public string PostProcess(string text)
    {
        foreach (var function in PostProcessing.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
        {
            var f = function;
            if (!f.Contains('.', StringComparison.Ordinal))
                f = Invariant($"Spune.Common.Functions.PostProcessFunction.{f}");
            if (MethodCaller.TryGetValue(f, [text], out var obj) && obj is string s)
                text = s;
        }

        return text;
    }
}