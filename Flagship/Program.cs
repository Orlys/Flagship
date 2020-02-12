
namespace Flagship
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            ulong flags1 = 0xB7F721006000010uL;

            var e = (BindingFlags)(flags1 & 67108863);
            flags1 >>= 26;
            System.Diagnostics.Debug.WriteLine("[BindingFlags] e: " + e);
            var d = (CallingConventions)(flags1 & 127);
            flags1 >>= 7;
            System.Diagnostics.Debug.WriteLine("[CallingConventions] d: " + d);
            var c = (MemberTypes)(flags1 & 255);
            flags1 >>= 8;
            System.Diagnostics.Debug.WriteLine("[MemberTypes] c: " + c);
            var b = (DayOfWeek)(flags1 & 7);
            flags1 >>= 3;
            System.Diagnostics.Debug.WriteLine("[DayOfWeek] b: " + b);
            var a = (FieldAttributes)(flags1 & 65535);
            System.Diagnostics.Debug.WriteLine("[FieldAttributes] a: " + a);

            var encoded = Flags.Encode
            (
                ("a", Enumerate.Create<FieldAttributes>().All),
                ("b", DayOfWeek.Monday),
                ("c", MemberTypes.Method),
                ("d", CallingConventions.Standard),
                ("e", BindingFlags.DoNotWrapExceptions | BindingFlags.Public)
            );

            var stub = encoded.GenerateDecodeStub(isDebug: true);
            Console.WriteLine(stub);
        }
    }
}