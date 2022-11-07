using System;
using System.IO;
using JetBrains.Annotations;

namespace fNbt
{
	internal class ByteCountingStream : Stream
	{
		public ByteCountingStream([NotNull] Stream stream)
		{
			this.baseStream = stream;
		}
		public override void Flush()
		{
			this.baseStream.Flush();
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.baseStream.Seek(offset, origin);
		}
		public override void SetLength(long value)
		{
			this.baseStream.SetLength(value);
		}
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
		public override bool CanRead
		{
			get
			{
				return this.baseStream.CanRead;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return this.baseStream.CanSeek;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return this.baseStream.CanWrite;
			}
		}
		public override long Length
		{
			get
			{
				return this.baseStream.Length;
			}
		}
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
		public long BytesRead { get; private set; }
		public long BytesWritten { get; private set; }
		private readonly Stream baseStream;
		private bool readingOneByte;
		private bool writingOneByte;
		private bool readingManyBytes;
		private bool writingManyBytes;
	}
}
