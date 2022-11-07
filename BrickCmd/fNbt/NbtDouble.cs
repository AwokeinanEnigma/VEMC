using JetBrains.Annotations;
using System;
using System.Text;

namespace fNbt
{
    public sealed class NbtDouble : NbtTag
    {
        public override NbtTagType TagType => NbtTagType.Double;
        public double Value { get; set; }
        public NbtDouble()
        {
        }
        public NbtDouble(double value) : this(null, value)
        {
        }
        public NbtDouble([CanBeNull] string tagName) : this(tagName, 0.0)
        {
        }
        public NbtDouble([CanBeNull] string tagName, double value)
        {
            name = tagName;
            Value = value;
        }
        public NbtDouble([NotNull] NbtDouble other)
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
                readStream.ReadDouble();
                return false;
            }
            Value = readStream.ReadDouble();
            return true;
        }
        internal override void SkipTag(NbtBinaryReader readStream)
        {
            readStream.ReadDouble();
        }
        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.Double);
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
            return new NbtDouble(this);
        }
        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            sb.Append("TAG_Double");
            if (!string.IsNullOrEmpty(base.Name))
            {
                sb.AppendFormat("(\"{0}\")", base.Name);
            }
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
