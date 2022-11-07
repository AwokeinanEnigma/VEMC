using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace fNbt
{
	internal sealed class ZLibStream : DeflateStream
	{
		public int Checksum
		{
			get
			{
				return this.adler32B * 65536 + this.adler32A;
			}
		}
		private void UpdateChecksum([NotNull] IList<byte> data, int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				this.adler32A = (this.adler32A + (int)data[offset + i]) % 65521;
				this.adler32B = (this.adler32B + this.adler32A) % 65521;
			}
		}
		public ZLibStream([NotNull] Stream stream, CompressionMode mode, bool leaveOpen) : base(stream, mode, leaveOpen)
		{
		}
		public override void Write(byte[] array, int offset, int count)
		{
			this.UpdateChecksum(array, offset, count);
			base.Write(array, offset, count);
		}
		private const int ChecksumModulus = 65521;
		private int adler32A = 1;
		private int adler32B;
	}
}
