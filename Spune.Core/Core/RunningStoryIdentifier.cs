//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Core.Core;

/// <summary>
/// Represents an identifier in a running story.
/// </summary>
public class RunningStoryIdentifier : IEquatable<RunningStoryIdentifier>
{
    /// <summary>
    /// Constructor of class RunningStoryIdentifier.
    /// </summary>
    public RunningStoryIdentifier()
    {
        Identifier = string.Empty;
        Text = string.Empty;
    }

    /// <summary>
    /// Constructor of class RunningStoryIdentifier.
    /// </summary>
    /// <param name="identifier">Identifier to set.</param>
    /// <param name="text">Text to set.</param>
    public RunningStoryIdentifier(string identifier, string text)
    {
        Identifier = identifier;
        Text = text;
    }

    /// <summary>
    /// Identifier.
    /// </summary>
    public string Identifier { get; set; }
    /// <summary>
    /// Text for the identifier.
    /// </summary>
    public string Text { get; set; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RunningStoryIdentifier identifier && RunningStoryIdentifierEquals(this, identifier);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Identifier, Text);

    /// <inheritdoc />
    public bool Equals(RunningStoryIdentifier? other)
    {
        if (other == null) return false;
        return RunningStoryIdentifierEquals(this, other);
    }

    /// <summary>
    /// Determines whether two specified <see cref="RunningStoryIdentifier"/> objects have the same value.
    /// </summary>
    /// <param name="lhs">The first <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <param name="rhs">The second <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <returns>true if the value of <paramref name="lhs"/> is the same as the value of <paramref name="rhs"/>; otherwise, false.</returns>
    public static bool operator ==(RunningStoryIdentifier? lhs, RunningStoryIdentifier? rhs)
    {
        if (object.Equals(lhs, null) && object.Equals(rhs, null)) return true;
        if (object.Equals(lhs, null) && !object.Equals(rhs, null)) return false;
        if (!object.Equals(lhs, null) && object.Equals(rhs, null)) return false;
        return RunningStoryIdentifierEquals(lhs!, rhs!);
    }

    /// <summary>
    /// Determines whether two specified <see cref="RunningStoryIdentifier"/> objects have different values.
    /// </summary>
    /// <param name="lhs">The first <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <param name="rhs">The second <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <returns>true if the value of <paramref name="lhs"/> is the same as the value of <paramref name="rhs"/>; otherwise, false.</returns>
    public static bool operator !=(RunningStoryIdentifier? lhs, RunningStoryIdentifier? rhs)
    {
        if (object.Equals(lhs, null) && object.Equals(rhs, null)) return false;
        if (object.Equals(lhs, null) && !object.Equals(rhs, null)) return true;
        if (!object.Equals(lhs, null) && object.Equals(rhs, null)) return true;
        return !RunningStoryIdentifierEquals(lhs!, rhs!);
    }

    /// <summary>
    /// Checks if the identifier is empty.
    /// </summary>
    /// <returns>True if it is and false otherwise.</returns>
    public bool IsEmpty() => string.IsNullOrEmpty(Identifier);

    /// <summary>
    /// Compares two <see cref="RunningStoryIdentifier"/> objects for equality.
    /// </summary>
    /// <param name="lhs">The first <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <param name="rhs">The second <see cref="RunningStoryIdentifier"/> to compare.</param>
    /// <returns>
    /// <c>true</c> if the identifiers and texts of both <see cref="RunningStoryIdentifier"/> objects are equal; otherwise, <c>false</c>.
    /// </returns>
    static bool RunningStoryIdentifierEquals(RunningStoryIdentifier lhs, RunningStoryIdentifier rhs)
    {
        return string.Equals(lhs.Identifier, rhs.Identifier, StringComparison.Ordinal) && string.Equals(lhs.Text, rhs.Text, StringComparison.Ordinal);
    }
}
