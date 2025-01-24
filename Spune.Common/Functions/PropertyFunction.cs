//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia;

namespace Spune.Common.Functions;

/// <summary>
/// This class contains property functions.
/// </summary>
public static class PropertyFunction
{
    /// <summary>
    /// Tries to get the new value of a property change event if the property name matches the specified name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="e">The event arguments containing the property change information.</param>
    /// <param name="name">The name of the property to match.</param>
    /// <param name="value">When this method returns, contains the new value of the property if the name matches; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>
    /// <c>true</c> if the property name matches and the new value is of type <typeparamref name="T"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetPropertyNewValue<T>(AvaloniaPropertyChangedEventArgs e, string name, out T? value)
    {
        if (!string.Equals(e.Property.Name, name, StringComparison.Ordinal) || e.NewValue is not T t)
        {
            value = default;
            return false;
        }
        value = t;
        return true;
    }

    /// <summary>
    /// Tries to get the new value of a property change event if the property name matches the specified property.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="e">The event arguments containing the property change information.</param>
    /// <param name="property">The property to match.</param>
    /// <param name="value">When this method returns, contains the new value of the property if the name matches; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>
    /// <c>true</c> if the property name matches and the new value is of type <typeparamref name="T"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetPropertyNewValue<T>(AvaloniaPropertyChangedEventArgs e, AvaloniaProperty property, out T? value)
    {
        var name = property.Name;
        return TryGetPropertyNewValue(e, name, out value);
    }
}
