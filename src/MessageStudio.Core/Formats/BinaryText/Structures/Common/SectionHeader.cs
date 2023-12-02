﻿using MessageStudio.Core.Common;
using System.Runtime.InteropServices;

namespace MessageStudio.Core.Formats.BinaryText.Structures.Common;

[StructLayout(LayoutKind.Sequential, Size = 12)]
public readonly partial struct SectionHeader : IReversable
{
    public const int LayoutSize = 12;

    public readonly int Size;

    public static void Reverse(in Span<byte> buffer)
    {
        buffer[0..4].Reverse();
    }
}
