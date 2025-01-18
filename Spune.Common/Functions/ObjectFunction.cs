//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
using System.Reflection;

namespace Spune.Common.Functions;

/// <summary>This class contains a collection of object functions.</summary>
public static class ObjectFunction
{
	/// <summary>
	/// Copies (deep) the given object.
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <typeparam name="T">Type of the object.</typeparam>
	/// <returns>The copy.</returns>
	public static T Clone<T>(T? obj)
	{
		if (obj == null) return obj!;
		if (IsPrimitive(typeof(T))) return obj;
		var o = obj as object;
		return (T)Clone(o);
	}

	/// <summary>
	/// Copies (deep) the given object.
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <returns>The copy.</returns>
	static object Clone(object obj)
	{
		var result = InternalCopy(obj, new Dictionary<object, object>(new ReferenceEqualityComparer()));
		if (result is not Stream { CanSeek: true } stream) return result;
		try
		{
			stream.Position = 0;
		}
		catch (NotSupportedException)
		{
		}

		return result;
	}

	/// <summary>
	/// Checks if the given type is a primitive.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>True if it is and false otherwise.</returns>
	static bool IsPrimitive(Type type) => type == typeof(string) || type.IsValueType || type.IsPrimitive;

	/// <summary>
	/// Copies (deep) the given object. This is the internal version.
	/// </summary>
	/// <param name="obj">Object to copy.</param>
	/// <param name="visited">Dictionary of visited objects.</param>
	/// <returns>The copy.</returns>
	static object InternalCopy(object obj, IDictionary<object, object> visited)
	{
		var type = obj.GetType();
		if (visited.TryGetValue(obj, out var v)) return v;
		if (typeof(Delegate).IsAssignableFrom(type)) return obj;
		var cloneObject = CloneMethod.Invoke(obj, null)!;
		if (type.IsArray)
		{
			var arrayType = type.GetElementType()!;
			if (!IsPrimitive(arrayType))
			{
				var clonedArray = (Array)cloneObject;
				ForEach(clonedArray, (array, indices) =>
				{
					var val = clonedArray.GetValue(indices);
					if (val != null)
						array.SetValue(InternalCopy(val, visited), indices);
				});
			}
		}
		visited.Add(obj, cloneObject);
		CopyFields(obj, visited, cloneObject, type);
		CopyBaseTypePrivateFields(obj, visited, cloneObject, type);
		return cloneObject;
	}

	/// <summary>
	/// Copies the private fields of the base type.
	/// </summary>
	/// <param name="obj">Object to act on.</param>
	/// <param name="visited">Dictionary of visited objects.</param>
	/// <param name="cloneObject">The cloned (copied) object.</param>
	/// <param name="typeToReflect">Type to use for reflection.</param>
	static void CopyBaseTypePrivateFields(object obj, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
	{
		if (typeToReflect.BaseType == null)
			return;

		CopyBaseTypePrivateFields(obj, visited, cloneObject, typeToReflect.BaseType);
		CopyFields(obj, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
	}

	/// <summary>
	/// Copies the fields from a given object.
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <param name="visited">Dictionary with visited objects.</param>
	/// <param name="cloneObject">The cloned object.</param>
	/// <param name="typeToReflect">Type to reflect.</param>
	/// <param name="bindingFlags">Binding flags to use.</param>
	/// <param name="filter">Field function filter to use.</param>
	static void CopyFields(object obj, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool>? filter = null)
	{
		foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
		{
			if (filter != null && !filter(fieldInfo)) continue;
			if (IsPrimitive(fieldInfo.FieldType)) continue;
			var originalFieldValue = fieldInfo.GetValue(obj);
			if (originalFieldValue != null)
			{
				var clonedFieldValue = InternalCopy(originalFieldValue, visited);
				fieldInfo.SetValue(cloneObject, clonedFieldValue);
			}
			else
			{
				fieldInfo.SetValue(cloneObject, null);
			}
		}
	}

	/// <summary>
	/// For each function for the given array.
	/// </summary>
	/// <param name="array">Given array.</param>
	/// <param name="action">Action to perform.</param>
	static void ForEach(Array array, Action<Array, int[]> action)
	{
		if (array.LongLength == 0) return;
		var walker = new ArrayTraverse(array);
		do action(array, walker.Position);
		while (walker.Step());
	}

	/// <summary>
	/// The clone method reference for the object.
	/// </summary>
	static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;
}

/// <summary>
/// Reference equality comparer class.
/// </summary>
internal class ReferenceEqualityComparer : EqualityComparer<object>
{
	/// <inheritdoc />
	public override bool Equals(object? x, object? y) => ReferenceEquals(x, y);
	/// <inheritdoc />
	public override int GetHashCode(object obj) => obj.GetHashCode();
}

/// <summary>
/// Array traverse class.
/// </summary>
internal class ArrayTraverse
{
	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="array">Given array.</param>
	public ArrayTraverse(Array array)
	{
		_maxLengths = new int[array.Rank];
		for (var i = 0; i < array.Rank; ++i)
		{
			_maxLengths[i] = array.GetLength(i) - 1;
		}
		Position = new int[array.Rank];
	}

	/// <summary>
	/// Position .
	/// </summary>
	public int[] Position { get; set; }

	/// <summary>
	/// Step through.
	/// </summary>
	/// <returns>True if step succeeded and false otherwise.</returns>
	public bool Step()
	{
		for (var i = 0; i < Position.Length; ++i)
		{
			if (Position[i] >= _maxLengths[i]) continue;
			Position[i]++;
			for (var j = 0; j < i; j++)
			{
				Position[j] = 0;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Max length.
	/// </summary>
	readonly int[] _maxLengths;
}