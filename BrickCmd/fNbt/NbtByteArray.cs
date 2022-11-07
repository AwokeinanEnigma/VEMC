// Decompiled with JetBrains decompiler

using JetBrains.Annotations;
using System;
using System.IO;
using System.Text;

namespace fNbt
{
    public sealed class NbtByteArray : NbtTag
    {
        private static readonly byte[] ZeroArray = new byte[0];
        [NotNull]
        private byte[] bytes;

        public override NbtTagType TagType
        {
            get
            {
                return NbtTagType.ByteArray;
            }
        }

        [NotNull]
        public byte[] Value
        {
            get
            {
                return this.bytes;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.bytes = value;
            }
        }

        public NbtByteArray()
          : this((string)null)
        {
        }

        public NbtByteArray([NotNull] byte[] value)
          : this((string)null, value)
        {
        }

        public NbtByteArray([CanBeNull] string tagName)
        {
            this.name = tagName;
            this.bytes = NbtByteArray.ZeroArray;
        }

        public NbtByteArray([CanBeNull] string tagName, [NotNull] byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.name = tagName;
            this.bytes = (byte[])value.Clone();
        }

        public NbtByteArray([NotNull] NbtByteArray other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            this.name = other.name;
            this.bytes = (byte[])other.Value.Clone();
        }

        public byte this[int tagIndex]
        {
            get
            {
                return this.Value[tagIndex];
            }
            set
            {
                this.Value[tagIndex] = value;
            }
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            int num = readStream.ReadInt32();
            if (num < 0)
                throw new NbtFormatException("Negative length given in TAG_Byte_Array");
            if (readStream.Selector != null && !readStream.Selector((NbtTag)this))
            {
                readStream.Skip(num);
                return false;
            }
            this.Value = readStream.ReadBytes(num);
            if (this.Value.Length < num)
                throw new EndOfStreamException();
            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            int bytesToSkip = readStream.ReadInt32();
            if (bytesToSkip < 0)
                throw new NbtFormatException("Negative length given in TAG_Byte_Array");
            readStream.Skip(bytesToSkip);
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.ByteArray);
            if (this.Name == null)
                throw new NbtFormatException("Name is null");
            writeStream.Write(this.Name);
            this.WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(this.Value.Length);
            writeStream.Write(this.Value, 0, this.Value.Length);
        }

        public override object Clone()
        {
            return (object)new NbtByteArray(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
                sb.Append(indentString);
            sb.Append("TAG_Byte_Array");
            if (!string.IsNullOrEmpty(this.Name))
                sb.AppendFormat("(\"{0}\")", (object)this.Name);
            sb.AppendFormat(": [{0} bytes]", (object)this.bytes.Length);
        }
    }
}
