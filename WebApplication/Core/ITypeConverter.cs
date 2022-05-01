using System;

namespace WebApplication.Core
{
    /// <summary>
    /// Converts objects.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter.
        /// </summary>
        /// <param name="type">A Type that represents the type you want to convert from. </param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        bool CanConvertFrom(Type type);
    }
}
