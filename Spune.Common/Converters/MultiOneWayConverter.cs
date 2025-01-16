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
/// This class represents a multi-value one-way converter.
/// </summary>
public sealed class MultiOneWayConverter<TTarget> : IMultiValueConverter
{
	/// <summary>
	/// Convert method reference.
	/// </summary>
	readonly Func<IList<object?>, object?, CultureInfo, TTarget> _convert;

	/// <summary>
	/// Initializes a new instance of the <see cref="MultiOneWayConverter{TTarget}" /> class.
	/// </summary>
	/// <param name="convert">Convert function to pass.</param>
	MultiOneWayConverter(Func<IList<object?>, object?, CultureInfo, TTarget> convert)
    {
        _convert = convert;
    }

    /// <inheritdoc />
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (typeof(TTarget) != targetType)
            return BindingOperations.DoNothing;
        try
        {
            var result = _convert(values, parameter, culture);
            return result ?? BindingOperations.DoNothing;
        }
        catch (InvalidCastException)
        {
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// Gets the instance of this class.
    /// </summary>
    /// <param name="convert">Convert function to pass.</param>
    /// <returns>An instance of this class.</returns>
    public static MultiOneWayConverter<TTarget> GetInstance(Func<IList<object?>, object?, CultureInfo, TTarget> convert)
    {
        return new MultiOneWayConverter<TTarget>(convert);
    }
}