
namespace Flagship
{
    using System;

    public readonly struct EnumField
    {
        public EnumField(Enum e)
        {
            this.Value = e;
            this.Info = Enumerate.Create(e.GetType());
        }

        public readonly Enum Value;
        public readonly Enumerate Info;

        public static implicit operator Enum(EnumField unit)
        {
            return unit.Value;
        }
        public static implicit operator EnumField(Enum unit)
        {
            return new EnumField(unit);
        }

        public void Deconstruct(out Enum value, out Enumerate info)
        {
            value = this.Value;
            info = this.Info;
        }
    }
}