
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