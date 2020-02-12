
namespace Flagship
{
    public struct EnumVariable
    {
        public void Deconstruct(out string name, out EnumField value)
        {
            value = this.Value;
            name = this.Name;
        }

        public readonly string Name;
        public readonly EnumField Value;

        public EnumVariable(string name, EnumField value)
        {
            this.Name = name;
            this.Value = value;
        }

        public static implicit operator EnumVariable((string name, EnumField value) _)
        {
            return new EnumVariable(_.name, _.value);
        }
    }
}