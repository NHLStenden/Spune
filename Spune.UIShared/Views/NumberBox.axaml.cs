//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Spune.UIShared.Views;

/// <inheritdoc />
/// <summary>This class represents a number converter for the <see cref="NumberBox" /> control.</summary>
internal class NumberBoxConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d || targetType != typeof(string)) return value;
        return !double.IsNaN(d) ? d.ToString(CultureInfo.CurrentCulture) : "";
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s) return new ValidationResult("String format error");
        if (string.IsNullOrEmpty(s)) return BindingOperations.DoNothing;
        if (targetType == typeof(double) && double.TryParse(s, out var result)) return result;

        return new ValidationResult("String format error");
    }
}

/// <summary>A touch friendly number box.</summary>
public partial class NumberBox : TextBox
{
	/// <summary>
	/// The value property.
	/// </summary>
	public static readonly StyledProperty<double> NumberProperty = AvaloniaProperty.Register<NumberBox, double>(nameof(Number), defaultBindingMode: BindingMode.TwoWay);

	/// <summary>
	/// Allowed characters in the control.
	/// </summary>
	/// <returns>The regEx instance with the allowed characters.</returns>
	static readonly Regex Regex = new(GetRegEx());

	/// <inheritdoc />
	/// <summary>
	/// Initializes a new instance of the <see cref="NumberBox" /> class.
	/// </summary>
	public NumberBox()
    {
        InitializeComponent();
        AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
    }

	/// <summary>
	/// Gets or sets the number.
	/// </summary>
	/// <value>The number.</value>
	public double Number
    {
        get => GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>
    /// Receives the text input and applies the regEx filter.
    /// </summary>
    /// <param name="sender">Sender of the change.</param>
    /// <param name="e">Parameter containing the entered text.</param>
    static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!Regex.IsMatch(e.Text ?? string.Empty))
            e.Handled = true;
    }

    /// <summary>
    /// Gets the regEx expression for this class.
    /// </summary>
    /// <returns>The regEx expression.</returns>
    static string GetRegEx()
    {
        var x = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        return Invariant($"^[0-9-+{x}]*$");
    }
}