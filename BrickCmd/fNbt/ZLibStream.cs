using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace fNbt
{
	// Token: 0x0200001F RID: 31
	internal sealed class ZLibStream : DeflateStream
	{
		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001BC RID: 444 RVA: 0x00007866 File Offset: 0x00005A66
		public int Checksum
		{
			get
			{
				return this.adler32B * 65536 + this.adler32A;
			}
		}

		// Token: 0x060001BD RID: 445 RVA: 0x0000787C File Offset: 0x00005A7C
		private void UpdateChecksum([NotNull] IList<byte> data, int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				this.adler32A = (this.adler32A + (int)data[offset + i]) % 65521;
				this.adler32B = (this.adler32B + this.adler32A) % 65521;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x000078CA File Offset: 0x00005ACA
		public ZLibStream([NotNull] Stream stream, CompressionMode mode, bool leaveOpen) : base(stream, mode, leaveOpen)
		{
		}

		// Token: 0x060001BF RID: 447 RVA: 0x000078DC File Offset: 0x00005ADC
		public override void Write(byte[] array, int offset, int count)
		{
			this.UpdateChecksum(array, offset, count);
			base.Write(array, offset, count);
		}

		// Token: 0x04000077 RID: 119
		private const int ChecksumModulus = 65521;

		// Token: 0x04000078 RID: 120
		private int adler32A = 1;

		// Token: 0x04000079 RID: 121
		private int adler32B;
	}
}
