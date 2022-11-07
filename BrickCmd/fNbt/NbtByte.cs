using JetBrains.Annotations;
using System;
using System.Text;

namespace fNbt
{
    public sealed class NbtByte : NbtTag
    {
        public override NbtTagType TagType => NbtTagType.Byte;
        public byte Value { get; set; }
        public NbtByte()
        {
        }
        public NbtByte(byte value) : this(null, value)
        {
        }
        public NbtByte([CanBeNull] string tagName) : this(tagName, 0)
        {
        }
        public NbtByte([CanBeNull] string tagName, byte value)
        {
            name = tagName;
            Value = value;
        }
        public NbtByte([NotNull] NbtByte other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            name = other.name;
            Value = other.Value;
        }
        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            if (readStream.Selector != null && !readStream.Selector(this))
            {
                readStream.ReadByte();
                return false;
            }
            Value = readStream.ReadByte();
            return true;
        }
        internal override void SkipTag(NbtBinaryReader readStream)
        {
            readStream.ReadByte();
        }
        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.Byte);
            if (base.Name == null)
            {
                throw new NbtFormatException("Name is null");
            }
            writeStream.Write(base.Name);
            writeStream.Write(Value);
        }
        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(Value);
        }
        public override object Clone()
        {
            return new NbtByte(this);
        }
        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            sb.Append("TAG_Byte");
            if (!string.IsNullOrEmpty(base.Name))
            {
                sb.AppendFormat("(\"{0}\")", base.Name);
            }
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
