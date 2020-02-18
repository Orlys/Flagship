
namespace Flagship
{
    using System;

    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class Enumeration
    {
        private static readonly Dictionary<Type, Enumeration> s_table;
        static Enumeration()
        {
            s_table = new Dictionary<Type, Enumeration>();
        }

        public static Enumeration Create<TEnum>() where TEnum : struct, Enum => Create(typeof(TEnum));

        public static Enumeration Create(Type enumType)
        {
            if(!s_table.TryGetValue(enumType, out var e))
            {
                e = new Enumeration(enumType);
                s_table.Add(enumType, e);
            }
            return e;
        }

        public bool HasFlags { get; }
        public bool IsUnsignedType { get; }
        public Type UnderlyingType { get; }
        public Type EnumType { get; }
        public TypeCode TypeCode { get; }
        public int Size { get; }
        public Enum[] Values { get; }
        public Enum Min { get; }
        public Enum Max { get; }
        public int Shift { get; }
        public ulong Mask { get; }
        public Enum All { get; }

        public static Enum ComplementedAnd(Enum subtracted, Enum minus)
        { 
            var t = subtracted.GetType();
            if (t != minus.GetType())
                throw new ArgumentNullException(nameof(minus), "type mismatched");
            var underlying = Enum.GetUnderlyingType(t);

            var eSub = Expression.Parameter(underlying);
            var eMinus = Expression.Parameter(underlying); 

            var body = Expression.OnesComplement(eMinus) as Expression;
            body = Expression.And(eSub, body);
            body = Expression.Convert(body, t);

            var expr = Expression.Lambda(body, eSub, eMinus).Compile();
            return expr.DynamicInvoke(subtracted, minus) as Enum;
        } 

        public static Enum Or(Enum left, Enum right)
        {
            var t = left.GetType();
            if (t != right.GetType())
                throw new ArgumentNullException(nameof(right), "type mismatched");

            var underlying = Enum.GetUnderlyingType(t);

            var enumType = Expression.Parameter(underlying);
            var enumR = Expression.Parameter(underlying);

            var body = Expression.Or(enumType, enumR) as Expression;
            body = Expression.Convert(body, t);

            var expr = Expression.Lambda(body, enumType, enumR).Compile(); 
            return expr.DynamicInvoke(left, right) as Enum;
        }
         
     

        private Enumeration(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new NotSupportedException();

            var typeCode = Type.GetTypeCode(enumType);
            if (typeCode < TypeCode.SByte || typeCode > TypeCode.UInt64)
                throw new NotSupportedException();

            this.TypeCode = typeCode;
            this.IsUnsignedType = (typeCode & TypeCode.Object) == TypeCode.Empty;

            this.EnumType = enumType;
            this.HasFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
            this.UnderlyingType = Enum.GetUnderlyingType(enumType);

            this.Size = Marshal.SizeOf(this.UnderlyingType);


            var values = Enum.GetValues(enumType) ;
            switch (values.Length)
            {
                case 0:
                    { 
                        this.Values = values.Cast<Enum>().ToArray();
                        this.Min = null;
                        this.Max = null;
                        break;
                    }
                case 1:
                    {
                        this.Values = values.Cast<Enum>().ToArray();
                        var index0 = (Enum)values.GetValue(0);
                        this.Min = index0;
                        this.Max = index0;
                        if (this.HasFlags)
                            this.All = index0;
                        break;
                    }
                default:
                    {
                        var hashset = new SortedSet<Enum>();

                        if (this.HasFlags)
                        {
                            var orAssign = new Func<Enum, Enum, Enum>(Or);
                            var defaultEnum = Activator.CreateInstance(this.EnumType) as Enum;
                            var max = defaultEnum;
                            this.All = defaultEnum;
                            foreach (Enum v in values)
                            {
                                this.All = orAssign(this.All, v);
                                if (hashset.Add(v))
                                    if (v.CompareTo(defaultEnum) > 0)
                                        max = orAssign(max, v);
                            }
                            this.Max = max;
                            this.Values = hashset.ToArray();
                            this.Min = this.Values.GetValue(0) as Enum;
                        }
                        else
                        {
                            foreach (Enum v in values)
                                hashset.Add(v);

                            this.Values = hashset.ToArray();
                            this.Min = this.Values.GetValue(0) as Enum;
                            this.Max = this.Values.GetValue(this.Values.Length - 1) as Enum;
                        }
                        
                        this.Shift = (int)Math.Ceiling(Math.Log((double)(dynamic)this.Max, 2.0));
                        this.Mask = (1uL << this.Shift) - 1;

                        break;
                    }
            }
        }



    }
}