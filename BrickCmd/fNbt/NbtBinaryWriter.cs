using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000008 RID: 8
	internal sealed class NbtBinaryWriter
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002520 File Offset: 0x00000720
		public Stream BaseStream
		{
			get
			{
				this.stream.Flush();
				return this.stream;
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002534 File Offset: 0x00000734
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

		// Token: 0x06000029 RID: 41 RVA: 0x000025A2 File Offset: 0x000007A2
		public void Write(byte value)
		{
			this.stream.WriteByte(value);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000025B0 File Offset: 0x000007B0
		public void Write(NbtTagType value)
		{
			this.stream.WriteByte((byte)value);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000025C0 File Offset: 0x000007C0
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

		// Token: 0x0600002C RID: 44 RVA: 0x00002618 File Offset: 0x00000818
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

		// Token: 0x0600002D RID: 45 RVA: 0x000026A4 File Offset: 0x000008A4
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

		// Token: 0x0600002E RID: 46 RVA: 0x00002798 File Offset: 0x00000998
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

		// Token: 0x0600002F RID: 47 RVA: 0x00002828 File Offset: 0x00000A28
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

		// Token: 0x06000030 RID: 48 RVA: 0x00002920 File Offset: 0x00000B20
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

		// Token: 0x06000031 RID: 49 RVA: 0x00002A1C File Offset: 0x00000C1C
		public void Write(byte[] data, int offset, int count)
		{
			int num;
			for (int i = 0; i < count; i += num)
			{
				num = Math.Min(536870912, count - i);
				this.stream.Write(data, offset + i, num);
			}
		}

		// Token: 0x0400000E RID: 14
		public const int MaxWriteChunk = 536870912;

		// Token: 0x0400000F RID: 15
		private const int BufferSize = 256;

		// Token: 0x04000010 RID: 16
		private const int MaxBufferedStringLength = 64;

		// Token: 0x04000011 RID: 17
		private static readonly UTF8Encoding Encoding = new UTF8Encoding(false, true);

		// Token: 0x04000012 RID: 18
		private readonly Encoder encoder = NbtBinaryWriter.Encoding.GetEncoder();

		// Token: 0x04000013 RID: 19
		private readonly Stream stream;

		// Token: 0x04000014 RID: 20
		private readonly byte[] buffer = new byte[256];

		// Token: 0x04000015 RID: 21
		private readonly bool swapNeeded;
	}
}
