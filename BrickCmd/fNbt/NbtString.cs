using JetBrains.Annotations;
using System;
using System.Text;

namespace fNbt
{
    public sealed class NbtString : NbtTag
    {
        public override NbtTagType TagType => NbtTagType.String;
        [NotNull]
        public string Value
        {
            get => stringVal;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                stringVal = value;
            }
        }
        public NbtString()
        {
        }
        public NbtString([NotNull] string value) : this(null, value)
        {
        }
        public NbtString([CanBeNull] string tagName, [NotNull] string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            name = tagName;
            Value = value;
        }
        public NbtString([NotNull] NbtString other)
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
                readStream.SkipString();
                return false;
            }
            Value = readStream.ReadString();
            return true;
        }
        internal override void SkipTag(NbtBinaryReader readStream)
        {
            readStream.SkipString();
        }
        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.String);
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
            return new NbtString(this);
        }
        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append(indentString);
            }
            sb.Append("TAG_String");
            if (!string.IsNullOrEmpty(base.Name))
            {
                sb.AppendFormat("(\"{0}\")", base.Name);
            }
            sb.Append(": \"");
            sb.Append(Value);
            sb.Append('"');
        }
        [NotNull]
        private string stringVal = "";
    }
}
