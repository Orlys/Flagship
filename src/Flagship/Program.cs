
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



        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args, new Dictionary<string, string>
            {
                { "-p", "path" },
            });

            var config = builder.Build();
            if (!(config["path"] is string path))
            {
                Console.WriteLine("[Error] path not selected");
                Environment.Exit(-1);
                return;
            }
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

                const char selectedChar = '+';
                const char emptyChar = ' ';
                const string prefix = "    ";

                var sp = prefix.Length + 1; // 1 = "[".length
                foreach (var t in types)
                {
                    Console.CursorVisible = false;

                    var valueCache = Activator.CreateInstance(t) as Enum; 
                    var e = Enumeration.Create(t);
                    Console.WriteLine($"[Select] type: { t }{(e.HasFlags ? ", multi-selectable" : ", non-flags")}");

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


                        //if (e.HasFlags)
                        //{
                        //    Console.WriteLine("continue select? [Y/N]");
                        //} 
                        if (Naming(out var name))
                        {
                            list.Enqueue((name, valueCache));
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

            }

            Console.CursorVisible = true;

            if (list.Count < 2)
            {
                Console.WriteLine("[Warning] not enough selected types for generate flags.");
                goto restart;
            }

            foreach (var item in list)
            {
                Console.WriteLine($"[Selected]: { item.Name }: { item.Field.Info.EnumType.Name }.{item.Field.Value}");
            }
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
                .Positive(() =>
                {
                    Clipboard.SetText(code);
                    Console.WriteLine();
                })
                .Negative(() =>
                {
                    Console.WriteLine(Environment.NewLine);
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
               .Positive(() =>
               {
                   list.Clear();
                   Console.Clear();
               })
               .Build()
               .Ask())
                goto start;

        }


    }
}