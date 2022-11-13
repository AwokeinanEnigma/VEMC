

using JetBrains.Annotations;
using System;
using System.Text;

namespace fNbt
{
    public sealed class NbtIntArray : NbtTag
    {
        private static readonly int[] ZeroArray = new int[0];
        [NotNull]
        private int[] ints;

        public override NbtTagType TagType => NbtTagType.IntArray;

        [NotNull]
        public int[] Value
        {
            get => ints;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                ints = value;
            }
        }

        public NbtIntArray()
          : this((string)null)
        {
        }

        public NbtIntArray([NotNull] int[] value)
          : this(null, value)
        {
        }

        public NbtIntArray([CanBeNull] string tagName)
        {
            name = tagName;
            ints = NbtIntArray.ZeroArray;
        }

        public NbtIntArray([CanBeNull] string tagName, [NotNull] int[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            name = tagName;
            ints = (int[])value.Clone();
        }

        public NbtIntArray([NotNull] NbtIntArray other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            name = other.name;
            ints = (int[])other.Value.Clone();
        }

        public int this[int tagIndex]
        {
            get => Value[tagIndex];
            set => Value[tagIndex] = value;
        }

        internal override bool ReadTag(NbtBinaryReader readStream)
        {
            int length = readStream.ReadInt32();
            if (length < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Int_Array");
            }

            if (readStream.Selector != null && !readStream.Selector(this))
            {
                readStream.Skip(length * 4);
                return false;
            }
            Value = new int[length];
            for (int index = 0; index < length; ++index)
            {
                Value[index] = readStream.ReadInt32();
            }

            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            int num = readStream.ReadInt32();
            if (num < 0)
            {
                throw new NbtFormatException("Negative length given in TAG_Int_Array");
            }

            readStream.Skip(num * 4);
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.IntArray);
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
            for (int index = 0; index < Value.Length; ++index)
            {
                writeStream.Write(Value[index]);
            }
        }

        public override object Clone()
        {
            return new NbtIntArray(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
            {
                sb.Append(indentString);
            }

            sb.Append("TAG_Int_Array");
            if (!string.IsNullOrEmpty(Name))
            {
                sb.AppendFormat("(\"{0}\")", Name);
            }

            sb.AppendFormat(": [{0} ints]", ints.Length);
        }
    }
}
