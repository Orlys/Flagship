namespace Flagship
{
    using System.Collections.Generic;
    using System.Linq;

    public class Flags
    {

         
        public static Encoded Encode(EnumVariable first, EnumVariable second, params EnumVariable[] rest)
        {
            var list = new List<EnumVariable> { first, second };
            if (rest != null)
                list.AddRange(rest);

            return Encode(list);
        }
        public static Encoded Encode(IEnumerable<EnumVariable> variables)
        {
            if (!(variables is List<EnumVariable> list))
                list = variables?.ToList() ?? new List<EnumVariable>(0);

            list.Sort((x, y) => (x.Field.Info.TypeCode - y.Field.Info.TypeCode));


            var flags = new List<Session> { new Session() };
            var offset = 0;
            foreach (var field in list)
            {

                var (_, (value, info)) = field;
                var tmp = offset + info.Shift;

                if (tmp > 64)
                {
                    flags.Add(new Session());
                    offset ^= offset;
                }
                else
                {
                    offset = tmp;
                }
                flags[flags.Count - 1].Fields.Add(field);
                flags[flags.Count - 1].Flags <<= info.Shift;
                flags[flags.Count - 1].Flags |= (ulong)(dynamic)value;
            }

            flags.Reverse();

            return new Encoded(flags);
        }
    }
}