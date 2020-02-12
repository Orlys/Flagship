
namespace Flagship
{

    using System.Collections.Generic;

    internal sealed class Session
    {
        public ulong Flags = 0uL;
        public List<EnumVariable> Fields { get; } = new List<EnumVariable>();

    }
}