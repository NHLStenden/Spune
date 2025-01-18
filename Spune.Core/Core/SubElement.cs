//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace Spune.Core.Core;

/// <summary>
/// Represents a sub-element derived from the core `Element` class within the system.
/// </summary>
/// <remarks>
/// This class serves as a base for elements that are part of a parent-child relationship.
/// It provides functionality to set and retrieve the parent element.
/// </remarks>
public class SubElement : Element
{
    /// <summary>
    /// Represents the parent element of the current instance.
    /// </summary>
    /// <remarks>
    /// This variable holds a reference to the parent <see cref="Element" /> instance in the hierarchical structure.
    /// It is used to establish a relationship between a child <see cref="Element" /> and its parent, allowing
    /// navigation or interaction within the element tree.
    /// </remarks>
    Element? _parent;

    /// <summary>
    /// Sets the parent element for the current element.
    /// </summary>
    /// <param name="parent">The parent element to associate with the current element.</param>
    public void SetParent(Element parent) => _parent = parent;

    /// <summary>
    /// Retrieves the parent element of the current sub-element, if any.
    /// </summary>
    /// <returns>
    /// The parent element of the sub-element, or null if the sub-element does not have a parent.
    /// </returns>
    public Element? GetParent() => _parent;
}