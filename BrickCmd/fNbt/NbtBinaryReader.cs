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
            swapNeeded = BitConverter.IsLittleEndian == bigEndian;
        }

        public NbtTagType ReadTagType()
        {
            int num = ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }

            if (num > 11)
            {
                throw new NbtFormatException("NBT tag type out of range: " + num);
            }

            return (NbtTagType)num;
        }

        public override short ReadInt16()
        {
            return swapNeeded ? NbtBinaryReader.Swap(base.ReadInt16()) : base.ReadInt16();
        }

        public override int ReadInt32()
        {
            return swapNeeded ? NbtBinaryReader.Swap(base.ReadInt32()) : base.ReadInt32();
        }

        public override long ReadInt64()
        {
            return swapNeeded ? NbtBinaryReader.Swap(base.ReadInt64()) : base.ReadInt64();
        }

        public override float ReadSingle()
        {
            if (!swapNeeded)
            {
                return base.ReadSingle();
            }

            FillBuffer(4);
            Array.Reverse(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }

        public override double ReadDouble()
        {
            if (!swapNeeded)
            {
                return base.ReadDouble();
            }

            FillBuffer(8);
            Array.Reverse(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public override string ReadString()
        {
            short num1 = ReadInt16();
            if (num1 < 0)
            {
                throw new NbtFormatException("Negative string length given!");
            }

            if (num1 < stringConversionBuffer.Length)
            {
                int num2;
                for (int offset = 0; offset < num1; offset += num2)
                {
                    num2 = BaseStream.Read(stringConversionBuffer, offset, num1);
                    if (num2 == 0)
                    {
                        throw new EndOfStreamException();
                    }
                }
                return Encoding.UTF8.GetString(stringConversionBuffer, 0, num1);
            }
            byte[] bytes = ReadBytes(num1);
            if (bytes.Length < num1)
            {
                throw new EndOfStreamException();
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public void Skip(int bytesToSkip)
        {
            if (bytesToSkip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bytesToSkip));
            }

            if (BaseStream.CanSeek)
            {
                BaseStream.Position += bytesToSkip;
            }
            else
            {
                if (bytesToSkip == 0)
                {
                    return;
                }

                if (seekBuffer == null)
                {
                    seekBuffer = new byte[8192];
                }

                int num;
                for (int index = 0; index < bytesToSkip; index += num)
                {
                    num = BaseStream.Read(seekBuffer, 0, Math.Min(8192, bytesToSkip - index));
                    if (num == 0)
                    {
                        throw new EndOfStreamException();
                    }
                }
            }
        }

        private new void FillBuffer(int numBytes)
        {
            int offset = 0;
            do
            {
                int num = BaseStream.Read(buffer, offset, numBytes - offset);
                if (num == 0)
                {
                    throw new EndOfStreamException();
                }

                offset += num;
            }
            while (offset < numBytes);
        }

        public void SkipString()
        {
            short num = ReadInt16();
            if (num < 0)
            {
                throw new NbtFormatException("Negative string length given!");
            }

            Skip(num);
        }

        [DebuggerStepThrough]
        private static short Swap(short v)
        {
            return (short)(v >> 8 & byte.MaxValue | v << 8 & 65280);
        }

        [DebuggerStepThrough]
        private static int Swap(int v)
        {
            uint num = (uint)v;
            return (int)(num >> 24) & byte.MaxValue | (int)(num >> 8) & 65280 | (int)num << 8 & 16711680 | (int)num << 24 & -16777216;
        }

        [DebuggerStepThrough]
        private static long Swap(long v)
        {
            return (NbtBinaryReader.Swap((int)v) & uint.MaxValue) << 32 | NbtBinaryReader.Swap((int)(v >> 32)) & uint.MaxValue;
        }

        [CanBeNull]
        public TagSelector Selector { get; set; }
    }
}
