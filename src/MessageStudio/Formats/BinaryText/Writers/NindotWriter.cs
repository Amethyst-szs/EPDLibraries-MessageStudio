using MessageStudio.Common;
using MessageStudio.Formats.BinaryText.Exceptions;
using MessageStudio.Formats.BinaryText.Parsers;
using MessageStudio.Formats.BinaryText.Structures;
using MessageStudio.Formats.BinaryText.Writers;
using Revrs;
using System.Text;

namespace MessageStudio.Formats.BinaryText;

public class NindotWriter
{
    public Endianness Endianness { get; set; } = Endianness.Little;
    public TextEncoding Encoding { get; set; } = TextEncoding.Unicode;

    // Requires a stream to write the output binary
    // Dictionary key holds the labels
    // KeyValuePair byte[] contains buffer representation of text
    // KeyValuePair string contains attribute data
    public unsafe void ToBinary(in Stream stream, in Dictionary<string, KeyValuePair<byte[], string>> dict,
        TextEncoding? encoding = null, Endianness? endianness = null)
    {
        endianness ??= Endianness;
        encoding ??= Encoding;
        
        RevrsWriter writer = new(stream, endianness.Value);
        ushort sectionCount = 0;
        bool isUsingATR1 = dict.Any(x => !string.IsNullOrEmpty(x.Value.Value));

        writer.Seek(sizeof(MsbtHeader));

        // Sort by the attributes if using ATR so that every
        // null/empty attribute is at the end
        Dictionary<string, KeyValuePair<byte[], string>> sorted = isUsingATR1
            ? dict
                .OrderBy(x => x.Value.Value)
                .OrderBy(x => string.IsNullOrEmpty(x.Value.Value))
                .ToDictionary(x => x.Key, x => x.Value)
            : dict;

        MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, Msbt.LBL1_MAGIC, () => {
            LabelSectionWriter.Write(ref writer, sorted.Keys);
        });

        if (isUsingATR1) {
            MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, Msbt.ATR1_MAGIC, () => {
                AttributeSectionWriter.Write(
                    ref writer, encoding.Value, sorted.Select(x => x.Value.Value).ToArray());
            });
        }

        MsbtSectionHeader.WriteSection(ref writer, ref sectionCount, Msbt.TXT2_MAGIC, () => {
            TextSectionWriter.WriteByte(ref writer, encoding.Value, sorted.Values.Select(x => x.Key).ToArray());
        });

        stream.SetLength(writer.Position);

        MsbtHeader header = new(
            magic: Msbt.MAGIC,
            byteOrderMark: Endianness.Big,
            encoding: encoding.Value,
            version: 3,
            sectionCount: sectionCount,
            fileSize: (uint)writer.Position
        );

        writer.Seek(0);
        writer.Write<MsbtHeader, MsbtHeader.Reverser>(header);
    }
}