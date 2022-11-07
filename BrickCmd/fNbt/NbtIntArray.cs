// Decompiled with JetBrains decompiler

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

        public override NbtTagType TagType
        {
            get
            {
                return NbtTagType.IntArray;
            }
        }

        [NotNull]
        public int[] Value
        {
            get
            {
                return this.ints;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.ints = value;
            }
        }

        public NbtIntArray()
          : this((string)null)
        {
        }

        public NbtIntArray([NotNull] int[] value)
          : this((string)null, value)
        {
        }

        public NbtIntArray([CanBeNull] string tagName)
        {
            this.name = tagName;
            this.ints = NbtIntArray.ZeroArray;
        }

        public NbtIntArray([CanBeNull] string tagName, [NotNull] int[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.name = tagName;
            this.ints = (int[])value.Clone();
        }

        public NbtIntArray([NotNull] NbtIntArray other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            this.name = other.name;
            this.ints = (int[])other.Value.Clone();
        }

        public int this[int tagIndex]
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
            int length = readStream.ReadInt32();
            if (length < 0)
                throw new NbtFormatException("Negative length given in TAG_Int_Array");
            if (readStream.Selector != null && !readStream.Selector((NbtTag)this))
            {
                readStream.Skip(length * 4);
                return false;
            }
            this.Value = new int[length];
            for (int index = 0; index < length; ++index)
                this.Value[index] = readStream.ReadInt32();
            return true;
        }

        internal override void SkipTag(NbtBinaryReader readStream)
        {
            int num = readStream.ReadInt32();
            if (num < 0)
                throw new NbtFormatException("Negative length given in TAG_Int_Array");
            readStream.Skip(num * 4);
        }

        internal override void WriteTag(NbtBinaryWriter writeStream)
        {
            writeStream.Write(NbtTagType.IntArray);
            if (this.Name == null)
                throw new NbtFormatException("Name is null");
            writeStream.Write(this.Name);
            this.WriteData(writeStream);
        }

        internal override void WriteData(NbtBinaryWriter writeStream)
        {
            writeStream.Write(this.Value.Length);
            for (int index = 0; index < this.Value.Length; ++index)
                writeStream.Write(this.Value[index]);
        }

        public override object Clone()
        {
            return (object)new NbtIntArray(this);
        }

        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel)
        {
            for (int index = 0; index < indentLevel; ++index)
                sb.Append(indentString);
            sb.Append("TAG_Int_Array");
            if (!string.IsNullOrEmpty(this.Name))
                sb.AppendFormat("(\"{0}\")", (object)this.Name);
            sb.AppendFormat(": [{0} ints]", (object)this.ints.Length);
        }
    }
}
