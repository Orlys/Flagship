
namespace Flagship
{
    using System;

    public static class Extensions
    {
        public static T As<T>(this Enum @enum) where T : struct
        {
            return (T)(ValueType)@enum;
        }
        public static TEnum Cast<TEnum>(this Enum @enum) where TEnum : struct, Enum
        {
            return (TEnum)@enum;
        }

        public static EnumVariable WithName(this Enum e, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return new EnumVariable(name, e);
        }

        public static sbyte ToInt8(this Enum @enum) => (sbyte)(dynamic)@enum;
        public static byte ToUInt8(this Enum @enum) => (byte)(dynamic)@enum;
        public static short ToInt16(this Enum @enum) => (short)(dynamic)@enum;
        public static ushort ToUInt16(this Enum @enum) => (ushort)(dynamic)@enum;
        public static int ToInt32(this Enum @enum) => (int)(dynamic)@enum;
        public static uint ToUInt32(this Enum @enum) => (uint)(dynamic)@enum;
        public static long ToInt64(this Enum @enum) => (long)(dynamic)@enum;
        public static ulong ToUInt64(this Enum @enum) => (ulong)(dynamic)@enum;
    }
}