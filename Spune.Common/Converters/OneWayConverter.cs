//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Spune.Common.Converters;

/// <summary>
/// This class represents a one-way converter.
/// </summary>
public sealed class OneWayConverter<TSource, TTarget> : IValueConverter
{
    /// <summary>
    /// Convert method reference.
    /// </summary>
    readonly Func<TSource, object?, CultureInfo, TTarget> _convert;

    /// <summary>
    /// Initializes a new instance of the <see cref="OneWayConverter{TSource, TTarget}" /> class.
    /// </summary>
    /// <param name="convert">Convert function to pass.</param>
    OneWayConverter(Func<TSource, object?, CultureInfo, TTarget> convert)
    {
        _convert = convert;
    }

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((typeof(TSource).IsValueType && value == null) || typeof(TTarget) != targetType)
            return BindingOperations.DoNothing;
        try
        {
            return _convert((TSource)value!, parameter, culture);
        }
        catch (InvalidCastException)
        {
            return BindingOperations.DoNothing;
        }
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }

    /// <summary>
    /// Gets the instance of this class.
    /// </summary>
    /// <param name="convert">Convert function to pass.</param>
    /// <returns>An instance of this class.</returns>
    public static OneWayConverter<TSource, TTarget> GetInstance(Func<TSource, object?, CultureInfo, TTarget> convert)
    {
        return new OneWayConverter<TSource, TTarget>(convert);
    }
}