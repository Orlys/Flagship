
namespace Flagship
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using TextCopy;
    class Program
    {


        private static bool Naming(out string name)
        {
            Console.CursorVisible = true;
            name = null;
            var sb = new StringBuilder();
            Console.WriteLine("[Info] press 'esc' to leave or skip naming procedure");
            Console.WriteLine("[Info] variable name only accepted '0-9', 'a-z', 'A-Z' and '_'");
            Console.Write("[Input] name: ");
            var leftPoint = Console.CursorLeft;
            while (true)
            {

                var c = Console.ReadKey(); 

                if (c.Key == ConsoleKey.Enter)
                {
                    if (sb.Length > 0)
                    {
                        Console.CursorTop++;
                        Console.CursorLeft = 0;
                        if (char.IsDigit(sb[0]))
                            sb.Insert(0, '_');
                        name = sb.ToString();

                        Console.CursorVisible = false;
                        return true;
                    }

                    Console.CursorLeft = leftPoint;
                }
                else if (c.Key == ConsoleKey.Escape)
                {    
                    Console.Write(char.MaxValue);
                    Console.CursorVisible = false;
                    return false;
                }

                else if (char.IsLetterOrDigit(c.KeyChar) || c.KeyChar == '_')
                {
                    sb.Append(c.KeyChar);
                }
                else if (c.Key == ConsoleKey.Backspace)
                {
                    if (Console.CursorLeft < leftPoint)
                    {
                        Console.CursorLeft = leftPoint;
                        continue;
                    }
                    Console.Write(' ');
                    Console.CursorLeft--;
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);
                } 
                else
                {
                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;
                }
            }
        }
         
        const char selectedChar = '+';
        const char emptyChar = ' ';
        const string prefix = "    ";

        const int sp = 4 + 1; // 4 = prefix.Length, 1 = "[".length

        private static void HasFlags(Type t, Enumeration e, Action<EnumVariable> enqueue)
        {
            var valueCache = Activator.CreateInstance(t) as Enum;

            var datumPoint = Console.CursorTop;
            Console.WriteLine($"{prefix}[{ selectedChar }] skip");
            for (int i = 0; i < e.Values.Length; i++)
            {
                var value = e.Values[i];
                Console.WriteLine($"{prefix}[ ] { value } ({ Convert.ToDecimal(value) })");
            }
            var topPoint = Console.CursorTop - 1;
            Console.CursorTop = datumPoint;
            var list = new Dictionary<int, Enum>();
            r:
            Console.CursorLeft = sp;
            var cursor = Console.ReadKey();

            if (cursor.Key == ConsoleKey.UpArrow)
            {
                if (Console.CursorTop > datumPoint)
                {
                    Console.CursorLeft--;
                    Console.Write(emptyChar);
                    Console.CursorTop--;
                }
                Console.CursorLeft--;
                Console.Write(selectedChar);
                goto r;
            }
            else if (cursor.Key == ConsoleKey.DownArrow)
            {
                if (Console.CursorTop < topPoint)
                {
                    Console.CursorLeft--;
                    Console.Write(emptyChar);
                    Console.CursorTop++;
                }
                Console.CursorLeft--;
                Console.Write(selectedChar);
                goto r;
            }
            else if (cursor.KeyChar == selectedChar)
            {
                var top = Console.CursorTop;

                var index = top - datumPoint -1;
                if (index == -1)
                {
                    Console.CursorTop = top + e.Values.Length - 1;
                    goto r;
                }

                var c = Console.CursorLeft; 
                 
                Console.CursorLeft = 2;

                if (list.ContainsKey(index))
                {
                    Console.Write(emptyChar);
                    list.Remove(index);
                }
                else
                {
                    Console.Write(selectedChar);
                    list.Add(index, e.Values[index]);
                }
                Console.CursorLeft = c;
                goto r;
            }
            else if (cursor.Key == ConsoleKey.Enter)
            {
                var index = Console.CursorTop - datumPoint; 
                Console.CursorLeft = 0;

                Console.CursorTop = datumPoint + e.Values.Length +1;
                if (--index == -1)
                {
                    goto moveNext;
                } 

                valueCache = list.Values.Aggregate(valueCache, Enumeration.Or);
                
                if (Naming(out var name))
                {
                    enqueue((name, valueCache)); 
                }
                goto moveNext;
            }
            else
            {
                Console.CursorLeft = sp;
                Console.Write(selectedChar);
                goto r;
            }

        moveNext:
            Console.WriteLine();
        }

        private static void NonFlags(Type t, Enumeration e, Action<EnumVariable> enqueue)
        {
            var valueCache = Activator.CreateInstance(t) as Enum;

            var datumPoint = Console.CursorTop;
            Console.WriteLine($"{prefix}[{ selectedChar }] skip");
            for (int i = 0; i < e.Values.Length; i++)
            {
                var value = e.Values[i];
                Console.WriteLine($"{prefix}[ ] { value } ({ Convert.ToDecimal(value) })"); //{ (i == 0 ? selectedChar : ' ') }
            }
            var topPoint = Console.CursorTop - 1;
            Console.CursorTop = datumPoint;

            r:
            Console.CursorLeft = sp;
            var v = Console.ReadKey();
            if (v.Key == ConsoleKey.UpArrow)
            {
                if (Console.CursorTop > datumPoint)
                {
                    Console.CursorLeft--;
                    Console.Write(emptyChar);
                    Console.CursorTop--;
                }
                Console.CursorLeft--;
                Console.Write(selectedChar);
                goto r;
            }
            else if (v.Key == ConsoleKey.DownArrow)
            {
                if (Console.CursorTop < topPoint)
                {
                    Console.CursorLeft--;
                    Console.Write(emptyChar);
                    Console.CursorTop++;
                }
                Console.CursorLeft--;
                Console.Write(selectedChar);
                goto r;
            }
            else if (v.Key == ConsoleKey.Enter)
            {
                var index = Console.CursorTop - datumPoint;
                Console.CursorTop = topPoint + 1;
                Console.CursorLeft = 0;

                if (--index == -1)
                    goto moveNext;
                var selectedValue = e.Values[index];
                valueCache = Enumeration.Or(valueCache, selectedValue);
                if (Naming(out var name))
                {
                    enqueue((name, valueCache));
                }
                goto moveNext;
            }
            else
            {
                Console.CursorLeft = sp;
                Console.Write(selectedChar);
                goto r;
            }

            moveNext:
            Console.WriteLine();
        } 

        static void Main(string[] args)
        {
#if !TEST
            if(args.Length == 0)
            {
                Console.WriteLine("[Error] path not selected");
                Environment.Exit(-1);
                return;
            } 
            var path = args[0];
#else 
            var path = "./Test.cs";
#endif

            var file = new FileInfo(path);
            if (!file.Exists)
            {
                Console.WriteLine("[Error] file not found");
                Environment.Exit(-1);
                return;
            }
             
            var list = new Queue<EnumVariable>();
            start:

            using (var r = new StreamReader(file.OpenRead()))
            {
                var codePage = r.ReadToEnd();
                var temper = new Temper();
                var types = temper.Temp(codePage).ToArray();

                if (types.Length < 2)
                {
                    Console.WriteLine("[Error] not enough types in this file.");
                    Environment.Exit(-1);
                    return;
                }
                Console.CursorVisible = false;

                foreach (var t in types)
                {
                    var e = Enumeration.Create(t);
                    Console.WriteLine($"[Select] type: { t }, {(e.HasFlags ? "multi-selectable" : "non-flags")}");
                    if(e.HasFlags)
                        Console.WriteLine($"[Info] press '{selectedChar}' to select and unselect item");
                    var procedure = e.HasFlags ? new Action<Type, Enumeration, Action<EnumVariable>>(HasFlags) : NonFlags;

                    procedure(t, e, list.Enqueue);

                }
            }

            Console.CursorVisible = true;

            if (list.Count < 2)
            {
                Console.WriteLine("[Warning] not enough selected types for generate flags.");
                goto restart;
            }


            var table = new ConsoleTables.ConsoleTable("type", "variable name", "value", "has flags");
            foreach (var item in list)
            {
                table.AddRow(item.Field.Info.EnumType, item.Name, item.Field.Value, item.Field.Info.HasFlags);
            }
            table.Write(ConsoleTables.Format.Alternative);
             
            Console.WriteLine();
            var enc = Flags.Encode(list);


            var code = enc.GenerateDecodeStub(YesOrNoQuestion.Builder
                .Title("Choose")
                .Question("need debug?")
                .Build()
                .Ask());

            YesOrNoQuestion.Builder
                .Title("Choose")
                .Question("copy code?")
                .Positive(() => TextCopy.Clipboard.SetText(code))
                .Negative(() =>
                {
                    Console.WriteLine();
                    Console.WriteLine("===========================================");
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine(code);
                    Console.ResetColor();
                    Console.WriteLine("===========================================");
                    Console.WriteLine();
                })
                .Build()
                .Ask();


            restart:
            if (YesOrNoQuestion.Builder
               .Title("Choose")
               .Question("rebuild?")
               .Positive(new Action(Console.Clear) + list.Clear)
               .Build()
               .Ask())
                goto start;

        }


    }
}