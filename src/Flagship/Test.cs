
namespace TestFile
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Flags]
    public enum FlagsEnum
    {
        A,B
    }
    [FlagsAttribute]
    public enum FlagsAttributeEnum
    {
        A, B
    }
    [System.Flags]
    public enum SystemFlagsEnum
    {
        A, B
    }
    [System.FlagsAttribute]
    public enum SystemFlagsAttributeEnum
    {
        A, B
    }

    public enum TestEnum
    {
        A
    }
}
