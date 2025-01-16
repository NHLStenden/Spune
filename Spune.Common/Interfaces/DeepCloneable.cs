
namespace Spune.Common.Interfaces;

/// <summary>
/// Interface for objects that can be cloned. This creates a deep copy of the object.
/// </summary>
/// <typeparam name="T">The type of object.</typeparam>
public interface IDeepCloneable<out T> where T : IDeepCloneable<T>
{
    /// <summary>
    /// Returns a deep copy of the object.
    /// </summary>
    /// <returns>A deep copy of the object.</returns>
    public T DeepClone();
}