﻿namespace MessageStudio.Formats.BinaryText;

public enum MsbtDuplicateKeyMode
{
    ThrowException,
    UseLastOccurrence,
    UseFirstOccurrence,
}

public class MsbtOptions
{
    public static readonly MsbtOptions Default = new();

    public bool IsGenerateTextStrings = true;

    public MsbtDuplicateKeyMode DuplicateKeyMode { get; set; } = MsbtDuplicateKeyMode.ThrowException;
}
