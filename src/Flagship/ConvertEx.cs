
namespace Flagship
{
    using System;

    public static class ConvertEx
    {
        public static object ChangeType(object from, Type to)
        {
            if (from is null)
                throw new ArgumentNullException(nameof(from));
            if (to is null)
                throw new ArgumentNullException(nameof(to));

            var toTypecode = Type.GetTypeCode(to);
            if (toTypecode < TypeCode.SByte || toTypecode > TypeCode.UInt64)
                throw new NotSupportedException(to.FullName);

            ValueType result;
            if (from is decimal d)
            {
                // force convertion 
                switch (toTypecode)
                {
                    case TypeCode.SByte: result = decimal.ToSByte(d); break;
                    case TypeCode.Byte: result = decimal.ToByte(d); break;
                    case TypeCode.Int16: result  = decimal.ToInt16(d); break;
                    case TypeCode.UInt16: result = decimal.ToUInt16(d); break;
                    case TypeCode.Int32: result = decimal.ToInt32(d); break;
                    case TypeCode.UInt32: result = decimal.ToUInt32(d); break;
                    case TypeCode.Int64: result = decimal.ToInt64(d); break;
                    case TypeCode.UInt64: result = decimal.ToUInt64(d); break;

                    default:
                        throw new InvalidCastException();
                }
                return result;
            }

            var fromT = from.GetType();
            var fromTypecode = Type.GetTypeCode(fromT);
            if (fromTypecode >= TypeCode.SByte && fromTypecode <= TypeCode.UInt64)
            {
                if (fromTypecode.Equals(toTypecode))
                    return from;
                unchecked
                {
                    switch (toTypecode)
                    {
                        case TypeCode.SByte: result = (sbyte)(ValueType)from; break;
                        case TypeCode.Byte: result = (byte)(ValueType)from; break;
                        case TypeCode.Int16: result = (short)(ValueType)from; break;
                        case TypeCode.UInt16: result = (ushort)(ValueType)from; break;
                        case TypeCode.Int32: result = (int)(ValueType)from; break;
                        case TypeCode.UInt32: result = (uint)(ValueType)from; break;
                        case TypeCode.Int64: result = (long)(ValueType)from; break;
                        case TypeCode.UInt64: result = (ulong)(ValueType)from; break;

                        default:
                            throw new InvalidCastException();
                    }
                }
                return result;
            }
            throw new NotSupportedException(fromT.FullName);
        }
    }
}