using System;
using System.ComponentModel;

namespace WebApplication.Core
{
    public class DefaultTypeConverter : ITypeConverter
    {
        private readonly Lazy<TypeConverter> _systemConverter;
        private readonly Type _type;
        private readonly bool _typeIsConvertible;

        public DefaultTypeConverter(Type type)
        {
            Guard.NotNull(type, nameof(type));
            _type = type;
            _typeIsConvertible = typeof(IConvertible).IsAssignableFrom(type);
            _systemConverter = new Lazy<TypeConverter>(() => TypeDescriptor.GetConverter(type));
        }

        public TypeConverter SystemConverter
        { 
            get 
            {
                if (_type == typeof(object))
                {
                    return null;
                }

                return _systemConverter.Value;
            }
        }
        public virtual bool CanConvertFrom(Type type)
        {
            if (_typeIsConvertible && typeof(IConvertible).IsAssignableFrom(type))
            { 
                return true;
            }

            if (SystemConverter != null)
            {
                return SystemConverter.CanConvertFrom(type);    
            }

            return false;
        }
    }
}
