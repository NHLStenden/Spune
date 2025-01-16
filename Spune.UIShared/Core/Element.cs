//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Spune.UIShared.Core;

/// <summary>
/// Represents a core element within the system that includes functionality for handling media, text, and AI prompt
/// integration.
/// </summary>
/// <remarks>
/// This class provides methods and properties to manage media content, text content, and AI-generated prompts, as well
/// as
/// the ability to dispose of resources. It can be extended by other element-related entities.
/// </remarks>
public class Element : INotifyPropertyChanged
{
    /// <summary>
    /// The hint member.
    /// </summary>
    string _hint = string.Empty;

    /// <summary>
    /// The identifier member.
    /// </summary>
    string _identifier = string.Empty;

    /// <summary>
    /// The link member.
    /// </summary>
    string _link = string.Empty;

    /// <summary>
    /// The text member.
    /// </summary>
    string _text = string.Empty;

    /// <summary>
    /// Gets or sets the hint for the element.
    /// </summary>
    public string Hint
    {
        get => _hint;
        set
        {
            _hint = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the unique identifier for the chapter.
    /// This is a string value that is used to identify the chapter within a master story
    /// and can serve for navigation or referencing purposes.
    /// </summary>
    public string Identifier
    {
        get => _identifier;
        set
        {
            _identifier = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Represents a navigational link associated with the interaction.
    /// This link is typically used to determine the next chapter or step
    /// in the master story or workflow, allowing navigation logic to be implemented.
    /// </summary>
    public string Link
    {
        get => _link;
        set
        {
            _link = value;
            NotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the text content associated with the element.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            NotifyPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Determines whether the element contains a hint.
    /// </summary>
    /// <returns>True if has it, and false otherwise.</returns>
    public bool HasHint() => !string.IsNullOrEmpty(Hint);

    /// <summary>
    /// Determines whether the element contains any text, either in the AI prompt or the text property.
    /// </summary>
    /// <returns>
    /// True if either the ChatMessage or Text property is not null or empty; otherwise, false.
    /// </returns>
    public virtual bool HasText() => !string.IsNullOrEmpty(Text);

    /// <summary>
    /// Checks if there's an identifier.
    /// </summary>
    /// <returns>True is it has, and false otherwise.</returns>
    public bool HasIdentifier() => !string.IsNullOrEmpty(Identifier);

    /// <summary>
    /// Notify that the property has changed.
    /// </summary>
    /// <param name="propertyName">Name of property that changed.</param>
    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}