using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace fNbt
{
    internal sealed class ZLibStream : DeflateStream
    {
        public int Checksum => adler32B * 65536 + adler32A;
        private void UpdateChecksum([NotNull] IList<byte> data, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                adler32A = (adler32A + data[offset + i]) % 65521;
                adler32B = (adler32B + adler32A) % 65521;
            }
        }
        public ZLibStream([NotNull] Stream stream, CompressionMode mode, bool leaveOpen) : base(stream, mode, leaveOpen)
        {
        }
        public override void Write(byte[] array, int offset, int count)
        {
            UpdateChecksum(array, offset, count);
            base.Write(array, offset, count);
        }
        private const int ChecksumModulus = 65521;
        private int adler32A = 1;
        private int adler32B;
    }
}
