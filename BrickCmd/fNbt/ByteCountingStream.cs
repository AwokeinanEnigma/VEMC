using System;
using System.IO;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x02000002 RID: 2
	internal class ByteCountingStream : Stream
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public ByteCountingStream([NotNull] Stream stream)
		{
			this.baseStream = stream;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000205F File Offset: 0x0000025F
		public override void Flush()
		{
			this.baseStream.Flush();
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000206C File Offset: 0x0000026C
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.baseStream.Seek(offset, origin);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000207B File Offset: 0x0000027B
		public override void SetLength(long value)
		{
			this.baseStream.SetLength(value);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000208C File Offset: 0x0000028C
		public override int Read(byte[] buffer, int offset, int count)
		{
			this.readingManyBytes = true;
			int num = this.baseStream.Read(buffer, offset, count);
			this.readingManyBytes = false;
			if (!this.readingOneByte)
			{
				this.BytesRead += (long)num;
			}
			return num;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020CE File Offset: 0x000002CE
		public override void Write(byte[] buffer, int offset, int count)
		{
			this.writingManyBytes = true;
			this.baseStream.Write(buffer, offset, count);
			this.writingManyBytes = false;
			if (!this.writingOneByte)
			{
				this.BytesWritten += (long)count;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002104 File Offset: 0x00000304
		public override int ReadByte()
		{
			this.readingOneByte = true;
			int num = base.ReadByte();
			this.readingOneByte = false;
			if (num >= 0 && !this.readingManyBytes)
			{
				this.BytesRead += 1L;
			}
			return num;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002142 File Offset: 0x00000342
		public override void WriteByte(byte value)
		{
			this.writingOneByte = true;
			base.WriteByte(value);
			this.writingOneByte = false;
			if (!this.writingManyBytes)
			{
				this.BytesWritten += 1L;
			}
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002170 File Offset: 0x00000370
		public override bool CanRead
		{
			get
			{
				return this.baseStream.CanRead;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000A RID: 10 RVA: 0x0000217D File Offset: 0x0000037D
		public override bool CanSeek
		{
			get
			{
				return this.baseStream.CanSeek;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000B RID: 11 RVA: 0x0000218A File Offset: 0x0000038A
		public override bool CanWrite
		{
			get
			{
				return this.baseStream.CanWrite;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002197 File Offset: 0x00000397
		public override long Length
		{
			get
			{
				return this.baseStream.Length;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000D RID: 13 RVA: 0x000021A4 File Offset: 0x000003A4
		// (set) Token: 0x0600000E RID: 14 RVA: 0x000021B1 File Offset: 0x000003B1
		public override long Position
		{
			get
			{
				return this.baseStream.Position;
			}
			set
			{
				this.baseStream.Position = value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000F RID: 15 RVA: 0x000021BF File Offset: 0x000003BF
		// (set) Token: 0x06000010 RID: 16 RVA: 0x000021C7 File Offset: 0x000003C7
		public long BytesRead { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000021D0 File Offset: 0x000003D0
		// (set) Token: 0x06000012 RID: 18 RVA: 0x000021D8 File Offset: 0x000003D8
		public long BytesWritten { get; private set; }

		// Token: 0x04000001 RID: 1
		private readonly Stream baseStream;

		// Token: 0x04000002 RID: 2
		private bool readingOneByte;

		// Token: 0x04000003 RID: 3
		private bool writingOneByte;

		// Token: 0x04000004 RID: 4
		private bool readingManyBytes;

		// Token: 0x04000005 RID: 5
		private bool writingManyBytes;
	}
}
