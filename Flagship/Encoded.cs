
namespace Flagship
{
    using System;

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    public class Encoded
    {
        private readonly List<Session> _session;

        internal Encoded(List<Session> dtos)
        {
            this._session = dtos;
        }

        public string GenerateDecodeStub(bool isDebug = default)
        {
            var sb = new StringBuilder();
            var index = 0;

            var varDefs = new List<string>(); 
            foreach (var d in this._session)
            {
                var stack = new Stack<EnumVariable>(d.Fields);

                var flagName = "flags" + ++index;
                varDefs.Add($"{flagName} = 0x{d.Flags.ToString("X")}uL");

                while (stack.TryPop(out var variable))
                {
                    var typeName = variable.Value.Info.EnumType.Name;
                    var mask = variable.Value.Info.Mask;
                    sb.AppendLine($"var {variable.Name} = ({typeName})({flagName} & {mask});");

                    if (stack.Count > 0)
                        sb.AppendLine($"{flagName} >>= {variable.Value.Info.Shift};");

                    if (isDebug)
                        sb.AppendLine($"System.Diagnostics.Debug.WriteLine(\"[{variable.Value.Info.EnumType.Name}] {variable.Name}: \" + {variable.Name});");
                }
            }
            sb.Insert(0, $"ulong { string.Join(", ", varDefs) };\r\n\r\n");
            return sb.ToString();
        }
    }
}