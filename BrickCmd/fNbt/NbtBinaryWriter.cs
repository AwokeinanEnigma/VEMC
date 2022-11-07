using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	internal sealed class NbtBinaryWriter
	{
		public Stream BaseStream
		{
			get
			{
				this.stream.Flush();
				return this.stream;
			}
		}
		public NbtBinaryWriter([NotNull] Stream input, bool bigEndian)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (!input.CanWrite)
			{
				throw new ArgumentException("Given stream must be writable", "input");
			}
			this.stream = input;
			this.swapNeeded = (BitConverter.IsLittleEndian == bigEndian);
		}
		public void Write(byte value)
		{
			this.stream.WriteByte(value);
		}
		public void Write(NbtTagType value)
		{
			this.stream.WriteByte((byte)value);
		}
		public void Write(short value)
		{
			if (this.swapNeeded)
			{
				this.buffer[0] = (byte)(value >> 8);
				this.buffer[1] = (byte)value;
			}
			else
			{
				this.buffer[0] = (byte)value;
				this.buffer[1] = (byte)(value >> 8);
			}
			this.stream.Write(this.buffer, 0, 2);
		}
		public void Write(int value)
		{
			if (this.swapNeeded)
			{
				this.buffer[0] = (byte)(value >> 24);
				this.buffer[1] = (byte)(value >> 16);
				this.buffer[2] = (byte)(value >> 8);
				this.buffer[3] = (byte)value;
			}
			else
			{
				this.buffer[0] = (byte)value;
				this.buffer[1] = (byte)(value >> 8);
				this.buffer[2] = (byte)(value >> 16);
				this.buffer[3] = (byte)(value >> 24);
			}
			this.stream.Write(this.buffer, 0, 4);
		}
		public void Write(long value)
		{
			if (this.swapNeeded)
			{
				this.buffer[0] = (byte)(value >> 56);
				this.buffer[1] = (byte)(value >> 48);
				this.buffer[2] = (byte)(value >> 40);
				this.buffer[3] = (byte)(value >> 32);
				this.buffer[4] = (byte)(value >> 24);
				this.buffer[5] = (byte)(value >> 16);
				this.buffer[6] = (byte)(value >> 8);
				this.buffer[7] = (byte)value;
			}
			else
			{
				this.buffer[0] = (byte)value;
				this.buffer[1] = (byte)(value >> 8);
				this.buffer[2] = (byte)(value >> 16);
				this.buffer[3] = (byte)(value >> 24);
				this.buffer[4] = (byte)(value >> 32);
				this.buffer[5] = (byte)(value >> 40);
				this.buffer[6] = (byte)(value >> 48);
				this.buffer[7] = (byte)(value >> 56);
			}
			this.stream.Write(this.buffer, 0, 8);
		}
		public unsafe void Write(float value)
		{
			ulong num = (ulong)(*(uint*)(&value));
			if (this.swapNeeded)
			{
				this.buffer[0] = (byte)(num >> 24);
				this.buffer[1] = (byte)(num >> 16);
				this.buffer[2] = (byte)(num >> 8);
				this.buffer[3] = (byte)num;
			}
			else
			{
				this.buffer[0] = (byte)num;
				this.buffer[1] = (byte)(num >> 8);
				this.buffer[2] = (byte)(num >> 16);
				this.buffer[3] = (byte)(num >> 24);
			}
			this.stream.Write(this.buffer, 0, 4);
		}
		public unsafe void Write(double value)
		{
			ulong num = (ulong)(*(long*)(&value));
			if (this.swapNeeded)
			{
				this.buffer[0] = (byte)(num >> 56);
				this.buffer[1] = (byte)(num >> 48);
				this.buffer[2] = (byte)(num >> 40);
				this.buffer[3] = (byte)(num >> 32);
				this.buffer[4] = (byte)(num >> 24);
				this.buffer[5] = (byte)(num >> 16);
				this.buffer[6] = (byte)(num >> 8);
				this.buffer[7] = (byte)num;
			}
			else
			{
				this.buffer[0] = (byte)num;
				this.buffer[1] = (byte)(num >> 8);
				this.buffer[2] = (byte)(num >> 16);
				this.buffer[3] = (byte)(num >> 24);
				this.buffer[4] = (byte)(num >> 32);
				this.buffer[5] = (byte)(num >> 40);
				this.buffer[6] = (byte)(num >> 48);
				this.buffer[7] = (byte)(num >> 56);
			}
			this.stream.Write(this.buffer, 0, 8);
		}
		public unsafe void Write([NotNull] string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			int byteCount = NbtBinaryWriter.Encoding.GetByteCount(value);
			this.Write((short)byteCount);
			if (byteCount <= 256)
			{
				NbtBinaryWriter.Encoding.GetBytes(value, 0, value.Length, this.buffer, 0);
				this.stream.Write(this.buffer, 0, byteCount);
				return;
			}
			int num = 0;
			int num2;
			for (int i = value.Length; i > 0; i -= num2)
			{
				num2 = ((i > 64) ? 64 : i);
				int bytes;
				fixed (char* ptr = value)
				{
					fixed (byte* ptr2 = this.buffer)
					{
						bytes = this.encoder.GetBytes(ptr + num, num2, ptr2, 256, num2 == i);
					}
				}
				this.stream.Write(this.buffer, 0, bytes);
				num += num2;
			}
		}
		public void Write(byte[] data, int offset, int count)
		{
			int num;
			for (int i = 0; i < count; i += num)
			{
				num = Math.Min(536870912, count - i);
				this.stream.Write(data, offset + i, num);
			}
		}
		public const int MaxWriteChunk = 536870912;
		private const int BufferSize = 256;
		private const int MaxBufferedStringLength = 64;
		private static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);
		private readonly Encoder encoder = NbtBinaryWriter.Encoding.GetEncoder();
		private readonly Stream stream;
		private readonly byte[] buffer = new byte[256];
		private readonly bool swapNeeded;
	}
}
