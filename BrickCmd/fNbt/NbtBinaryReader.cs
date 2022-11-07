// Decompiled with JetBrains decompiler

using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace fNbt
{
    internal sealed class NbtBinaryReader : BinaryReader
    {
        private readonly byte[] buffer = new byte[8];
        private readonly byte[] stringConversionBuffer = new byte[64];
        private const int SeekBufferSize = 8192;
        private byte[] seekBuffer;
        private readonly bool swapNeeded;

        public NbtBinaryReader([NotNull] Stream input, bool bigEndian)
          : base(input)
        {
            this.swapNeeded = BitConverter.IsLittleEndian == bigEndian;
        }

        public NbtTagType ReadTagType()
        {
            int num = (int)this.ReadByte();
            if (num < 0)
                throw new EndOfStreamException();
            if (num > 11)
                throw new NbtFormatException("NBT tag type out of range: " + (object)num);
            return (NbtTagType)num;
        }

        public override short ReadInt16()
        {
            return this.swapNeeded ? NbtBinaryReader.Swap(base.ReadInt16()) : base.ReadInt16();
        }

        public override int ReadInt32()
        {
            return this.swapNeeded ? NbtBinaryReader.Swap(base.ReadInt32()) : base.ReadInt32();
        }

        public override long ReadInt64()
        {
            return this.swapNeeded ? NbtBinaryReader.Swap(base.ReadInt64()) : base.ReadInt64();
        }

        public override float ReadSingle()
        {
            if (!this.swapNeeded)
                return base.ReadSingle();
            this.FillBuffer(4);
            Array.Reverse((Array)this.buffer, 0, 4);
            return BitConverter.ToSingle(this.buffer, 0);
        }

        public override double ReadDouble()
        {
            if (!this.swapNeeded)
                return base.ReadDouble();
            this.FillBuffer(8);
            Array.Reverse((Array)this.buffer);
            return BitConverter.ToDouble(this.buffer, 0);
        }

        public override string ReadString()
        {
            short num1 = this.ReadInt16();
            if (num1 < (short)0)
                throw new NbtFormatException("Negative string length given!");
            if ((int)num1 < this.stringConversionBuffer.Length)
            {
                int num2;
                for (int offset = 0; offset < (int)num1; offset += num2)
                {
                    num2 = this.BaseStream.Read(this.stringConversionBuffer, offset, (int)num1);
                    if (num2 == 0)
                        throw new EndOfStreamException();
                }
                return Encoding.UTF8.GetString(this.stringConversionBuffer, 0, (int)num1);
            }
            byte[] bytes = this.ReadBytes((int)num1);
            if (bytes.Length < (int)num1)
                throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }

        public void Skip(int bytesToSkip)
        {
            if (bytesToSkip < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToSkip));
            if (this.BaseStream.CanSeek)
            {
                this.BaseStream.Position += (long)bytesToSkip;
            }
            else
            {
                if (bytesToSkip == 0)
                    return;
                if (this.seekBuffer == null)
                    this.seekBuffer = new byte[8192];
                int num;
                for (int index = 0; index < bytesToSkip; index += num)
                {
                    num = this.BaseStream.Read(this.seekBuffer, 0, Math.Min(8192, bytesToSkip - index));
                    if (num == 0)
                        throw new EndOfStreamException();
                }
            }
        }

        private new void FillBuffer(int numBytes)
        {
            int offset = 0;
            do
            {
                int num = this.BaseStream.Read(this.buffer, offset, numBytes - offset);
                if (num == 0)
                    throw new EndOfStreamException();
                offset += num;
            }
            while (offset < numBytes);
        }

        public void SkipString()
        {
            short num = this.ReadInt16();
            if (num < (short)0)
                throw new NbtFormatException("Negative string length given!");
            this.Skip((int)num);
        }

        [DebuggerStepThrough]
        private static short Swap(short v)
        {
            return (short)((int)v >> 8 & (int)byte.MaxValue | (int)v << 8 & 65280);
        }

        [DebuggerStepThrough]
        private static int Swap(int v)
        {
            uint num = (uint)v;
            return (int)(num >> 24) & (int)byte.MaxValue | (int)(num >> 8) & 65280 | (int)num << 8 & 16711680 | (int)num << 24 & -16777216;
        }

        [DebuggerStepThrough]
        private static long Swap(long v)
        {
            return ((long)NbtBinaryReader.Swap((int)v) & (long)uint.MaxValue) << 32 | (long)NbtBinaryReader.Swap((int)(v >> 32)) & (long)uint.MaxValue;
        }

        [CanBeNull]
        public TagSelector Selector { get; set; }
    }
}
