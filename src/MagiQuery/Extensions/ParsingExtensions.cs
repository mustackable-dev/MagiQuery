using System.Globalization;
using System.Linq.Expressions;

namespace MagiQuery.Extensions;

internal static partial class InternalExtensions
{
    private static ConstantExpression ToCharConstant(this string value, bool isNullable = false) =>
        isNullable ? Expression.Constant(value[0], typeof(char?)) : Expression.Constant(value[0], typeof(char));
    private static ConstantExpression? ToSByteConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (sbyte.TryParse(value, culture, out sbyte result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(sbyte?));
            }

            return Expression.Constant(result, typeof(sbyte));
        }
        return null;
    }
    
    private static ConstantExpression? ToShortConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (short.TryParse(value, culture, out short result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(short?));
            }

            return Expression.Constant(result, typeof(short));
        }
        return null;
    }
    
    private static ConstantExpression? ToUShortConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (ushort.TryParse(value, culture, out ushort result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(ushort?));
            }

            return Expression.Constant(result, typeof(ushort));
        }
        return null;
    }
    
    private static ConstantExpression? ToByteConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (byte.TryParse(value, culture, out byte result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(byte?));
            }

            return Expression.Constant(result, typeof(byte));
        }
        return null;
    }
    
    private static ConstantExpression? ToInt32Constant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (Int32.TryParse(value, culture, out Int32 result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(int?));
            }

            return Expression.Constant(result, typeof(int));
        }
        return null;
    }
    
    private static ConstantExpression? ToUInt32Constant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (uint.TryParse(value, culture, out uint result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(uint?));
            }

            return Expression.Constant(result, typeof(uint));
        }
        return null;
    }
    
    private static ConstantExpression? ToInt64Constant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (long.TryParse(value, culture, out long result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(long?));
            }

            return Expression.Constant(result, typeof(long));
        }
        return null;
    }
    
    private static ConstantExpression? ToUInt64Constant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (ulong.TryParse(value, culture, out ulong result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(ulong?));
            }

            return Expression.Constant(result, typeof(ulong));
        }
        
        return null;
    }
    
    private static ConstantExpression? ToIntPtrConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (IntPtr.TryParse(value, culture, out IntPtr result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(nint?));
            }

            return Expression.Constant(result, typeof(nint));
        }
        return null;
    }
    
    private static ConstantExpression? ToUIntPtrConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (UIntPtr.TryParse(value, culture, out UIntPtr result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(nuint?));
            }

            return Expression.Constant(result, typeof(nuint));
        }
        return null;
    }
    
    private static ConstantExpression? ToFloatConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (float.TryParse(value, culture, out float result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(float?));
            }

            return Expression.Constant(result, typeof(float));
        }
        return null;
    }
    
    private static ConstantExpression? ToDoubleConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (double.TryParse(value, culture, out double result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(double?));
            }

            return Expression.Constant(result, typeof(double));
        }
        return null;
    }
    
    private static ConstantExpression? ToDecimalConstant(this string value, CultureInfo culture, bool isNullable = false)
    {
        if (decimal.TryParse(value, culture, out decimal result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(decimal?));
            }

            return Expression.Constant(result, typeof(decimal));
        }
        return null;
    }
    
    private static ConstantExpression? ToBoolConstant(this string value, bool isNullable = false)
    {
        if (bool.TryParse(value, out bool result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(bool?));
            }
            
            return Expression.Constant(result, typeof(bool));
        }

        return null;
    }
    
    private static ConstantExpression? ToEnumConstant(this string value, Type underlyingType, bool isNullable = false)
    {
        if (Enum.TryParse(underlyingType, value, true, out object? result))
        {
            if (isNullable)
            {
                return Expression.Constant(result, typeof(Nullable<>).MakeGenericType(underlyingType));
            }
            
            return Expression.Constant(result, underlyingType);
        }

        return null;
    }
    
    private static ConstantExpression? ToDateTimeConstant(
        this string value,
        CultureInfo culture,
        string? exactParseFormat = null,
        DateTimeKind? overrideDateTimeKind = null,
        bool isNullable = false)
    {
        Type returnType = isNullable ? typeof(DateTime?) : typeof(DateTime);
        if (exactParseFormat is not null && 
            DateTime.TryParseExact(value, exactParseFormat, culture, DateTimeStyles.None,
                out DateTime exactParsedValue))
        {
            if(overrideDateTimeKind is not null)
                exactParsedValue = DateTime.SpecifyKind(exactParsedValue, overrideDateTimeKind.Value);
            
            return Expression.Constant(exactParsedValue, returnType);
        }
            
        if(DateTime.TryParse(value, culture, out DateTime parsedValue))
        {
            if(overrideDateTimeKind is not null)
                parsedValue = DateTime.SpecifyKind(parsedValue, overrideDateTimeKind.Value);
            
            return Expression.Constant(parsedValue, returnType);
        }

        return null;
    }
    
    private static ConstantExpression? ToDateTimeOffsetConstant(
        this string value,
        CultureInfo culture,
        string? exactParseFormat = null,
        bool isNullable = false)
    {
        Type returnType = isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
        if (exactParseFormat is not null && 
            DateTimeOffset.TryParseExact(value, exactParseFormat, culture, DateTimeStyles.None,
                out DateTimeOffset exactParsedValue))
        {
            return Expression.Constant(exactParsedValue, returnType);
        }
            
        if(DateTimeOffset.TryParse(value, culture, out DateTimeOffset parsedValue))
        {
            return Expression.Constant(parsedValue, returnType);
        }

        return null;
    }
    
    private static ConstantExpression? ToDateOnlyConstant(
        this string value,
        CultureInfo culture,
        string? exactParseFormat = null,
        bool isNullable = false)
    {
        Type returnType = isNullable ? typeof(DateOnly?) : typeof(DateOnly);
        if (exactParseFormat is not null && 
            DateOnly.TryParseExact(value, exactParseFormat, culture, DateTimeStyles.None,
                out DateOnly exactParsedValue))
        {
            return Expression.Constant(exactParsedValue, returnType);
        }
            
        if(DateOnly.TryParse(value, culture, out DateOnly parsedValue))
        {
            return Expression.Constant(parsedValue, returnType);
        }

        return null;
    }
    
    private static ConstantExpression? ToTimeSpanConstant(
        this string value,
        CultureInfo culture,
        string? exactParseFormat = null,
        bool isNullable = false)
    {
        Type returnType = isNullable ? typeof(TimeSpan?) : typeof(TimeSpan);
        if (exactParseFormat is not null && 
            TimeSpan.TryParseExact(value, exactParseFormat, culture, TimeSpanStyles.None,
                out TimeSpan exactParsedValue))
        {
            return Expression.Constant(exactParsedValue, returnType);
        }
            
        if(TimeSpan.TryParse(value, culture, out TimeSpan parsedValue))
        {
            return Expression.Constant(parsedValue, returnType);
        }

        return null;
    }
    
    private static ConstantExpression? ToTimeOnlyConstant(
        this string value,
        CultureInfo culture,
        string? exactParseFormat = null,
        bool isNullable = false)
    {
        Type returnType = isNullable ? typeof(TimeOnly?) : typeof(TimeOnly);
        if (exactParseFormat is not null && 
            TimeOnly.TryParseExact(value, exactParseFormat, culture, DateTimeStyles.None,
                out TimeOnly exactParsedValue))
        {
            return Expression.Constant(exactParsedValue, returnType);
        }
            
        if(TimeOnly.TryParse(value, culture, out TimeOnly parsedValue))
        {
            return Expression.Constant(parsedValue, returnType);
        }

        return null;
    }

    internal static Expression ConvertToIntIfEnum(this Expression expression, Type type)
        => type .IsEnum ? Expression.Convert(expression, typeof(int)) : expression;
}