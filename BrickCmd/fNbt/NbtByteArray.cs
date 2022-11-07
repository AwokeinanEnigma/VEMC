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

        public override NbtTagType TagType => NbtTagType.ByteArray;

        [NotNull]
        public byte[] Value
        {
            get => bytes;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                bytes = value;
            }
        }

        public NbtByteArray()
          : this((string)null)
        {
        }

        public NbtByteArray([NotNull] byte[] value)
          : this(null, value)
        {
        }

        public NbtByteArray([CanBeNull] string tagName)
        {
            name = tagName;
            bytes = NbtByteArray.ZeroArray;
        }

        public NbtByteArray([CanBeNull] string tagName, [NotNull] byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            name = tagName;
            bytes = (byte[])value.Clone();
        }

        public NbtByteArray([NotNull] NbtByteArray other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            name = other.name;
            bytes = (byte[])other.Value.Clone();
        }

        public byte this[int tagIndex]
        {
            get => Value[tagIndex];
            set => Value[tagIndex] = value;
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            int num = readStream.ReadInt32();
            if (num < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Byte_Array");
            }

            if (readStream.Selector != null && !readStream.Selector(this))
            {
                readStream.Skip(num);
                return false;
            }
            Value = readStream.ReadBytes(num);
            if (Value.Length < num)
            {
                throw new EndOfStreamException();
            }

            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            int bytesToSkip = readStream.ReadInt32();
            if (bytesToSkip < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Byte_Array");
            }

            readStream.Skip(bytesToSkip);
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.ByteArray);
            if (Name == null)
            {
                throw new NbtFormatException("Name is null");
            }

            writeStream.Write(Name);
            WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(Value.Length);
            writeStream.Write(Value, 0, Value.Length);
        }

        public override object Clone()
        {
            return new NbtByteArray(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
            {
                sb.Append(indentString);
            }

            sb.Append("TAG_Byte_Array");
            if (!string.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("(\"{0}\")", Name);
            }

            sb.AppendFormat(": [{0} bytes]", bytes.Length);
        }
    }
}
