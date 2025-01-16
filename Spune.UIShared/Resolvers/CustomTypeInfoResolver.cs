//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Spune.UIShared.Resolvers;

/// <summary>
/// A custom type info resolver that skips serialization of string properties with empty values.
/// </summary>
public class CustomTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    /// <summary>
    /// Gets the type information for the specified type, modifying the serialization behavior for string properties.
    /// </summary>
    /// <param name="type">The type to get information for.</param>
    /// <param name="options">Options to control the behavior during serialization.</param>
    /// <returns>The modified type information.</returns>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;
        foreach (var property in typeInfo.Properties)
        {
            if (property.PropertyType != typeof(string))
                continue;

            var originalShouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = (obj, value) => (originalShouldSerialize == null || originalShouldSerialize(obj, value)) && !string.IsNullOrEmpty((string)value!);
        }
        return typeInfo;
    }
}
