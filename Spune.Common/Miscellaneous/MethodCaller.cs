//--------------------------------------------------------------------------------------------------
// <copyright company="NHL Stenden">
//     Author: Martin Bosgra
//     Copyright © NHL Stenden. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Reflection;
using Spune.Common.Functions;

namespace Spune.Common.Miscellaneous;

/// <summary>
/// This class allows the calling of a C# method.
/// </summary>
public static class MethodCaller
{
	/// <summary>
	/// Tries to call the C# member.
	/// </summary>
	/// <param name="method">Method to call. It includes the namespace and class name.</param>
	/// <param name="parameters">The parameters.</param>
	/// <param name="result">[out] The result.</param>
	/// <returns>True if successful and false otherwise.</returns>
	public static bool TryGetValue(string method, object?[]? parameters, out object? result)
    {
        var elements = method.Split('.').ToArray();
        if (elements.Length < 2)
        {
            result = null;
            return false;
        }

        var lastIndex = method.LastIndexOf('.');

        if (lastIndex < 0)
        {
            result = null;
            return false;
        }

        var className = method[..lastIndex];
        var methodName = method[(lastIndex + 1)..];

        var classType = Type.GetType(className);
        if (classType == null)
        {
            result = null;
            return false;
        }

        var (strippedMethodName, methodParamCount) = ProcessMethod(methodName);
        if ((parameters == null && methodParamCount != 0) ||
            (parameters != null && methodParamCount != parameters.Length))
        {
            result = null;
            return false;
        }

        var m = classType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x =>
            string.Equals(x.Name, strippedMethodName, StringComparison.Ordinal) &&
            x.GetParameters().Length == methodParamCount);
        if (m == null)
        {
            result = null;
            return false;
        }

        result = m.Invoke(null, parameters);
        return true;
    }

	/// <summary>
	/// Processes a method call.
	/// </summary>
	/// <param name="method">The method to process.</param>
	/// <returns>The processed method with name and parameter count.</returns>
	static (string, int) ProcessMethod(string method)
    {
        var result = method.Trim();

        var end = method.LastIndexOf(')');
        if (end < 0)
            return (method, 0);
        var begin = result.IndexOf('(');
        if (begin < 0) return (method, 0);
        var parametersAsString = result.Substring(begin + 1, end - begin - 1);
        var parameters = ParserFunction.CSharpParameterSplit(parametersAsString);
        method = method[..begin];
        return (method, parameters.Count);
    }
}