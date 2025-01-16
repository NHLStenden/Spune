//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Spune.UIShared.Views;

/// <summary>
/// Provides extension methods for controls.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    /// Identifies the PointerClick attached property.
    /// </summary>
    public static readonly AttachedProperty<EventHandler<RoutedEventArgs>> PointerClickProperty = AvaloniaProperty.RegisterAttached<Control, EventHandler<RoutedEventArgs>>("PointerClick", typeof(ControlExtensions));

    /// <summary>
    /// The is pressed member.
    /// </summary>
    static bool _isPressed;

    /// <summary>
    /// The static constructor of class ControlExtensions.
    /// </summary>
    static ControlExtensions() => PointerClickProperty.Changed.AddClassHandler<Control>(HandlePointerClickChanged);

    /// <summary>
    /// Gets the value of the PointerClick attached property for a given control.
    /// </summary>
    /// <param name="control">The control from which to get the property value.</param>
    /// <returns>The event handler for the PointerClick property.</returns>
    public static EventHandler<RoutedEventArgs> GetPointerClick(Control control) => control.GetValue(PointerClickProperty);
    /// <summary>
    /// Sets the value of the PointerClick attached property for a given control.
    /// </summary>
    /// <param name="control">The control on which to set the property value.</param>
    /// <param name="value">The event handler to set for the PointerClick property.</param>
    public static void SetPointerClick(Control control, EventHandler<RoutedEventArgs> value) => control.SetValue(PointerClickProperty, value);

    /// <summary>
    /// Handles changes to the PointerClick attached property change.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    static void HandlePointerClickChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is EventHandler<RoutedEventArgs>)
        {
            sender.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            sender.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        }
        else
        {
            sender.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            sender.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        }
    }

    /// <summary>
    /// Handles the PointerPressed event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            _isPressed = true;
    }

    /// <summary>
    /// Handles the PointerReleased event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (!_isPressed || e.InitialPressMouseButton != MouseButton.Left)
            return;

        _isPressed = false;
        if (!control.GetVisualsAt(e.GetPosition(control)).Any(c => control == c || control.IsVisualAncestorOf(c)))
            return;

        var eventHandler = control.GetValue(PointerClickProperty);
        eventHandler.Invoke(control, new RoutedEventArgs());
    }
}