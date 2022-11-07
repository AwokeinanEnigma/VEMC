using JetBrains.Annotations;
using System;
using System.Text;

namespace fNbt
{
    public sealed class NbtLong : NbtTag
    {
        public override NbtTagType TagType => NbtTagType.Long;
        public long Value { get; set; }
        public NbtLong()
        {
        }
        public NbtLong(long value) : this(null, value)
        {
        }
        public NbtLong(string tagName) : this(tagName, 0L)
        {
        }
        public NbtLong(string tagName, long value)
        {
            name = tagName;
            Value = value;
        }
        public NbtLong([NotNull] NbtLong other)
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
                readStream.ReadInt64();
                return false;
            }
            Value = readStream.ReadInt64();
            return true;
        }
        internal override void SkipTag(NbtBinaryReader readStream)
        {
            readStream.ReadInt64();
        }
        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.Long);
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
            return new NbtLong(this);
        }
        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            sb.Append("TAG_Long");
            if (!string.IsNullOrEmpty(base.Name))
            {
                sb.AppendFormat("(\"{0}\")", base.Name);
            }
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
