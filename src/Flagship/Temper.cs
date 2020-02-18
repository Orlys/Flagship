
namespace Flagship
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class Temper
    {  
        private static Delegate AddOneBuilder(Type target)
        {
            var arg = Expression.Parameter(target);
            var one = Expression.Convert(Expression.Constant(1), target);
            var body = Expression.Add(arg, one);
            return Expression.Lambda(body, arg).Compile();
        }
        private static Delegate MakeZeroBuilder(Type target)
        {
            var zero = Expression.Convert(Expression.Constant(0), target);
            return Expression.Lambda(zero).Compile();
        }

        private static Delegate TryParseBuilder(Type type)
        {
            var s = Expression.Parameter(typeof(string));
            var reft = type.MakeByRefType();
            var result = Expression.Parameter(reft);
            var tryParse = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, null, new[] { s.Type, reft }, null);
            Expression body = Expression.Call(tryParse, s, result);
            return Expression.Lambda(body, s, result).Compile();
        }
        private static readonly IReadOnlyDictionary<string, Type> s_typesMapper = new Dictionary<string, Type>
        {
            { "sbyte",     typeof(sbyte)   },   { typeof(sbyte).FullName,   typeof(sbyte)   },
            { "byte",      typeof(byte)    },   { typeof(byte).FullName,    typeof(byte)    },
            { "short",     typeof(short)   },   { typeof(short).FullName,   typeof(short)   },
            { "ushort",    typeof(ushort)  },   { typeof(ushort).FullName,  typeof(ushort)  },
            { "int",       typeof(int)     },   { typeof(int).FullName,     typeof(int)     },
            { "uint",      typeof(uint)    },   { typeof(uint).FullName,    typeof(uint)    },
            { "long",      typeof(long)    },   { typeof(long).FullName,    typeof(long)    },
            { "ulong",     typeof(ulong)   },   { typeof(ulong).FullName,   typeof(ulong)   }
        };

        public IEnumerable<Type> Temp(string codePage, CancellationToken cancellationToken = default)
        {
            var tree = CSharpSyntaxTree.ParseText(codePage, cancellationToken: cancellationToken);

            var unit = tree.GetCompilationUnitRoot();

            var tasks = unit
                .DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .Where(e => !e.GetDiagnostics().Any(x => x.Severity.Equals(DiagnosticSeverity.Error)));

            var q = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("_"), AssemblyBuilderAccess.RunAndCollect);
            var b = q.DefineDynamicModule("_");


            foreach (var i in tasks)
            {
                var underlyingType = i.BaseList is BaseListSyntax bls ? s_typesMapper[bls.Types[0].Type.ToString()] : typeof(int);

                var addOne = AddOneBuilder(underlyingType);
                var makeZero = MakeZeroBuilder(underlyingType);
                var tryParse = TryParseBuilder(underlyingType);
                 
                
                var enumBuilder = b.DefineEnum(i.Identifier.Text, TypeAttributes.Public, underlyingType);
                foreach (var a in i.AttributeLists.SelectMany(x => x.Attributes))
                {
                    switch (a.Name.ToString())
                    {
                        case "System.Flags":
                        case "System.FlagsAttribute":
                        case "FlagsAttribute":
                        case "Flags":
                            var attributeBuilder = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes), Array.Empty<object>());
                            enumBuilder.SetCustomAttribute(attributeBuilder);
                            break;
                    }
                }

                var indexer = new Dictionary<string, ValueType>();
                ValueType last = null;

                foreach (var field in i.Members)
                {
                    var literalName = field.Identifier.Text;
                    var memory = new Queue<MemberAccessExpressionSyntax>();
                    if (field.EqualsValue?.Value is ExpressionSyntax es)
                    {
                        if (es is PrefixUnaryExpressionSyntax || es is LiteralExpressionSyntax)
                        {
                            //underlyingType.

                            var argv = new object[2] { es.ToFullString(), null };
                            if ((bool)tryParse.DynamicInvoke(argv))
                            {
                                var literalValue = (ValueType)argv[1];
                                enumBuilder.DefineLiteral(literalName, literalValue);
                                indexer.Add(literalName, literalValue);
                                last = literalValue;
                            }
                            else
                                Console.WriteLine("[Warning] unresolved literal: " + es);
                        }
                        else if (es is MemberAccessExpressionSyntax ma)
                        {
                            if (ma.Expression is IdentifierNameSyntax id && literalName.Equals(id.Identifier.Text))
                            {
                                if (indexer.TryGetValue(ma.Name.Identifier.Text, out var literalValue))
                                {
                                    var f = enumBuilder.DefineLiteral(literalName, literalValue);
                                    indexer.Add(literalName, literalValue);
                                    last = literalValue;
                                }
                                else
                                    memory.Enqueue(ma);
                            }
                            else
                            {
                                Console.WriteLine("[Warning] unresolved member: " + ma);
                            }
                        }
                        else if (es is IdentifierNameSyntax id)
                        {
                            //indexer
                            if (indexer.TryGetValue(id.Identifier.Text, out var literalValue))
                            {
                                var f = enumBuilder.DefineLiteral(literalName, literalValue);
                                indexer.Add(literalName, literalValue);
                                last = literalValue;
                            }
                        }
                        //else if (es is CheckedExpressionSyntax checkWrapped &&
                        //    checkWrapped.Expression is CastExpressionSyntax ce)
                        //{
                        //    if (ce.Expression is LiteralExpressionSyntax le)
                        //    {
                        //        Console.WriteLine(le.Token.Text);
                        //    }

                        //    Console.WriteLine(ce);
                        //    Console.WriteLine(ce.Type);
                        //    Console.WriteLine(ce.Expression.GetType() + "*");
                        //}
                        else
                        {
                            Console.WriteLine("[Warning] unhandled expression node type:" + es.GetType().Name);
                        }
                    }
                    else
                    {
                        var literalValue = (ValueType)(last is ValueType v ? addOne.DynamicInvoke(v) : makeZero.DynamicInvoke());
                        var f = enumBuilder.DefineLiteral(literalName, literalValue);
                        indexer.Add(literalName, literalValue);
                        last = literalValue;
                    }

                    if (memory.Count > 0)
                    {
                        var resetControl = memory.Count;
                        while (memory.Count > 0)
                        {
                            if (resetControl < 0)
                            {
                                foreach (var er in memory)
                                {
                                    Console.WriteLine("[Warning] unhandled literal:" + er);
                                }
                                break;
                            }

                            var ma = memory.Dequeue();
                            if (indexer.TryGetValue(ma.Name.Identifier.Text, out var literalValue))
                            {
                                var f = enumBuilder.DefineLiteral(literalName, literalValue);
                                indexer.Add(literalName, literalValue);
                                last = literalValue;
                                resetControl = memory.Count;
                            }
                            else
                            {
                                resetControl--;
                                memory.Enqueue(ma);
                            }
                        }
                    }
                }
                 
                var e = enumBuilder.CreateTypeInfo();
                var et = e.AsType();

                //Console.WriteLine("[Info] created reflection type: " + et.FullName);
                yield return et;
            }
        }
    }
}