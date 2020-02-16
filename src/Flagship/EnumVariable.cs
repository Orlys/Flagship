
namespace Flagship
{
    public struct EnumVariable
    {
        public void Deconstruct(out string name, out EnumField value)
        {
            value = this.Field;
            name = this.Name;
        }

        public readonly string Name;
        public readonly EnumField Field;

        public EnumVariable(string name, EnumField value)
        {
            this.Name = name;
            this.Field = value;
        }

        public static implicit operator EnumVariable((string name, EnumField value) _)
        {
            return new EnumVariable(_.name, _.value);
        }
    }
}