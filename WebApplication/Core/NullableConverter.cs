using System;

namespace WebApplication.Core
{
    public class NullableConverter : DefaultTypeConverter
    {
        internal NullableConverter(Type type, Type elementType) : base(type)
        { 
        
        }
    }
}
